using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using spennyIRC.Core.IRC;

public class IrcClient : IIrcClient
{
    private TcpClient? _tcpClient;
    private Stream? _stream;
    private StreamReader? _reader;
    private CancellationTokenSource? _connectionCts;
    private CancellationTokenSource? _readCts;
    private Task? _readTask;
    private readonly SemaphoreSlim _sendSemaphore = new(1, 1);
    private bool _disposed;

    public Func<string, Task>? OnDataReceivedHandler { get; set; }
    public Func<string, Task>? OnDisconnectedHandler { get; set; }

    public async Task ConnectAsync(string server, int port, bool useSsl = false)
    {
#if DEBUG
        Debug.WriteLine($"ConnectAsync: Starting connection to {server}:{port} (SSL: {useSsl})");
#endif
        // Clean up any existing connection
        await DisconnectAsync().ConfigureAwait(false);

        _connectionCts = new CancellationTokenSource();

        try
        {
            _tcpClient = new TcpClient();

            // Connect with cancellation support
            using (CancellationTokenSource connectCts = CancellationTokenSource.CreateLinkedTokenSource(_connectionCts.Token))
            {
                connectCts.CancelAfter(TimeSpan.FromSeconds(60));
#if DEBUG
                Debug.WriteLine($"ConnectAsync: Attempting TCP connection...");
#endif
                await _tcpClient.ConnectAsync(server, port, connectCts.Token).ConfigureAwait(false);
            }

#if DEBUG
            Debug.WriteLine($"ConnectAsync: TCP connection established");
#endif

            // Check if we were cancelled during connection
            _connectionCts.Token.ThrowIfCancellationRequested();

            NetworkStream networkStream = _tcpClient.GetStream();

            // Set up the stream (SSL or regular)
            if (useSsl)
            {
#if DEBUG
                Debug.WriteLine($"ConnectAsync: Setting up SSL stream...");
#endif
                SslStream sslStream = new(networkStream, false, ValidateServerCertificate, null);
                await sslStream.AuthenticateAsClientAsync(server).ConfigureAwait(false);
                _stream = sslStream;
#if DEBUG
                Debug.WriteLine($"ConnectAsync: SSL authentication complete");
#endif
            }
            else
            {
                _stream = networkStream;
#if DEBUG
                Debug.WriteLine($"ConnectAsync: Using plain TCP stream");
#endif
            }

            // Check again for cancellation
            _connectionCts.Token.ThrowIfCancellationRequested();

            // Set up reader (no writer, use direct stream writes)
            _reader = new StreamReader(_stream, Encoding.UTF8, false, 2048, leaveOpen: true);

            // Start the read loop
            _readCts = new CancellationTokenSource();
            _readTask = Task.Run(() => ReadLoopAsync(_readCts.Token), _readCts.Token);

#if DEBUG
            Debug.WriteLine($"ConnectAsync: Connection successful, read loop started");
#endif
        }
        catch (OperationCanceledException ex)
        {
#if DEBUG
            Debug.WriteLine($"ConnectAsync: Operation cancelled - {ex.Message}");
#endif
            // Connection was cancelled, clean up
            await CleanupConnection().ConfigureAwait(false);
            throw new OperationCanceledException("Connection attempt was cancelled");
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"ConnectAsync: Error - {ex}");
#endif
            // Any other error, clean up and rethrow
            await CleanupConnection().ConfigureAwait(false);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
#if DEBUG
        Debug.WriteLine($"DisconnectAsync: Starting disconnect");
#endif
        // Cancel any ongoing connection attempt
        _connectionCts?.Cancel();

        // Cancel the read loop
        _readCts?.Cancel();

        // Wait for read task to complete (with timeout)
        if (_readTask != null && !_readTask.IsCompleted)
        {
            try
            {
                await _readTask.WaitAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
#if DEBUG
                Debug.WriteLine($"DisconnectAsync: Read task completed");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"DisconnectAsync: Read task wait error - {ex.Message}");
#endif
                // Ignore exceptions from cancellation or timeout
            }
        }

        await CleanupConnection().ConfigureAwait(false);
#if DEBUG
        Debug.WriteLine($"DisconnectAsync: Disconnect complete");
#endif
    }

    public async Task SendMessageAsync(string message)
    {
        if (_stream == null || _tcpClient?.Connected != true)
        {
#if DEBUG
            Debug.WriteLine($"SendMessageAsync: Not connected, cannot send: {message}");
#endif
            throw new InvalidOperationException("Not connected to IRC server");
        }

        // Ensure thread-safe sending
        await _sendSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message + "\r\n");
#if DEBUG
            Debug.WriteLine($">> {message.TrimEnd()}");
#endif
            await _stream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException || ex is OperationCanceledException)
        {
#if DEBUG
            Debug.WriteLine($"SendMessageAsync: Send error - {ex}");
#endif
            // Rethrow to notify the caller of the failure
            throw;
        }
        finally
        {
            _sendSemaphore.Release();
        }
    }

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        Debug.WriteLine($"ReadLoopAsync: Started");
#endif
        try
        {
            while (!cancellationToken.IsCancellationRequested && _reader != null && _tcpClient?.Connected == true)
            {
                string? line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                if (line == null)
                {
#if DEBUG
                    Debug.WriteLine($"ReadLoopAsync: Null line received, connection lost");
#endif
                    // Stream ended, connection lost
                    break;
                }

#if DEBUG
                Debug.WriteLine($"<< {line}");
#endif

                if (OnDataReceivedHandler != null)
                {
                    try
                    {
                        await OnDataReceivedHandler(line).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Debug.WriteLine($"ReadLoopAsync: OnDataReceivedHandler error - {ex}");
#endif
                        // Continue processing other messages even if handler fails
                    }
                }
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"ReadLoopAsync: Error - {ex}");
#endif
        }

#if DEBUG
        Debug.WriteLine($"ReadLoopAsync: Exiting, cancellation requested: {cancellationToken.IsCancellationRequested}");
#endif

        // Notify disconnection (always, matching old code behavior)
        if (OnDisconnectedHandler != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await OnDisconnectedHandler("Disconnected from the server.").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine(ex.ToString());
#endif
                }
            }, CancellationToken.None);
        }
    }

    /// <summary>
    /// Validates SSL server certificates. Currently accepts all certificates including self-signed.
    /// Modify this method to implement proper certificate validation.
    /// </summary>
    private bool ValidateServerCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
#if DEBUG
        Debug.WriteLine($"ValidateServerCertificate: Policy errors: {sslPolicyErrors}");
#endif
        // TODO: Implement proper certificate validation
        // For now, accept all certificates including self-signed
        return true;
    }

    private async Task CleanupConnection()
    {
#if DEBUG
        Debug.WriteLine($"CleanupConnection: Starting cleanup");
#endif
        try
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_stream != null)
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
            }

            _tcpClient?.Dispose();
#if DEBUG
            Debug.WriteLine($"CleanupConnection: Resources disposed");
#endif
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"CleanupConnection: Error during cleanup - {ex}");
#endif
            // Ignore cleanup errors
        }
        finally
        {
            _reader = null;
            _stream = null;
            _tcpClient = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
#if DEBUG
                Debug.WriteLine($"Dispose: Disposing resources");
#endif
                DisconnectAsync().GetAwaiter().GetResult();
                _sendSemaphore?.Dispose();
                _connectionCts?.Dispose();
                _readCts?.Dispose();
            }
            _disposed = true;
        }
    }
}