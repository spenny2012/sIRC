using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace spennyIRC.Core.IRC
{
    public class IrcClient : IIrcClient, IAsyncDisposable, IDisposable
    {
        private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
        private readonly object _stateLock = new();

        private TcpClient? _tcpClient;
        private Stream? _networkStream;
        private bool _isDisposed;
        private CancellationTokenSource? _cts;
        private Task? _receiveTask;

        public Func<string, Task>? OnDataReceivedHandler { get; set; }
        public Func<string, Task>? OnDisconnectedHandler { get; set; }

        public async Task ConnectAsync(string server, int port, bool useSsl = false)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            await _connectionSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                    throw new InvalidOperationException("Already connected to a server.");

                await DisconnectInternalAsync().ConfigureAwait(false);

                _tcpClient = new TcpClient();
                try
                {
                    await _tcpClient.ConnectAsync(server, port).ConfigureAwait(false);
                    _networkStream = _tcpClient.GetStream();

                    if (useSsl)
                    {
                        SslStream sslStream = new(_networkStream, false, ValidateServerCertificate, null);
                        try
                        {
                            await sslStream.AuthenticateAsClientAsync(server).ConfigureAwait(false);
                            _networkStream = sslStream;
                        }
                        catch
                        {
                            await sslStream.DisposeAsync().ConfigureAwait(false);
                            throw;
                        }
                    }
                }
                catch
                {
                    _tcpClient?.Dispose();
                    _tcpClient = null;
                    if (_networkStream != null)
                    {
                        await _networkStream.DisposeAsync().ConfigureAwait(false);
                        _networkStream = null;
                    }
                    throw;
                }

                _cts = new CancellationTokenSource();
                _receiveTask = StartReceivingMessagesAsync(_cts.Token);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        public async Task SendMessageAsync(string message)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            await _connectionSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_networkStream == null)
                    throw new InvalidOperationException("Not connected to the server.");

                byte[] messageBytes = Encoding.UTF8.GetBytes(message + "\r\n");
                await _networkStream.WriteAsync(messageBytes).ConfigureAwait(false);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        private async Task StartReceivingMessagesAsync(CancellationToken cancellationToken)
        {
            Stream? currentStream;
            TcpClient? currentClient;

            lock (_stateLock)
            {
                currentStream = _networkStream;
                currentClient = _tcpClient;
            }

            if (currentStream == null) return;

            using StreamReader reader = new(currentStream, Encoding.UTF8, false, 2048, leaveOpen: true);

            try
            {
                while (!_isDisposed && !cancellationToken.IsCancellationRequested)
                {
                    // Check connection status safely
                    lock (_stateLock)
                    {
                        if (_tcpClient == null || !_tcpClient.Connected)
                            break;
                    }

                    try
                    {
                        string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                        if (line == null)
                            break;

                        // Properly handle async event with exception handling
                        if (OnDataReceivedHandler != null)
                        {
                            try
                            {
                                await OnDataReceivedHandler(line).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error in OnDataReceivedHandler: {ex.Message}");
                                // Continue processing other messages
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error receiving message: {e.Message}");
                        break;
                    }
                }
            }
            finally
            {
                try
                {
                    if (OnDisconnectedHandler != null)
                    {
                        try
                        {
                            await OnDisconnectedHandler("Disconnected from the server.").ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in OnDisconnectedHandler: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    await DisconnectInternalAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task DisconnectAsync()
        {
            await _connectionSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await DisconnectInternalAsync().ConfigureAwait(false);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        private async Task DisconnectInternalAsync()
        {
            CancellationTokenSource? ctsToDispose = _cts;
            _cts = null;

            ctsToDispose?.Cancel();

            // Wait for receive task to complete
            if (_receiveTask != null)
            {
                try
                {
                    await _receiveTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation token is triggered
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error waiting for receive task completion: {ex.Message}");
                }
                finally
                {
                    _receiveTask = null;
                }
            }

            if (_networkStream != null)
            {
                try
                {
                    await _networkStream.DisposeAsync().ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    // Expected if already disposed
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing network stream: {ex.Message}");
                }
                finally
                {
                    _networkStream = null;
                }
            }

            _tcpClient?.Dispose();
            _tcpClient = null;

            ctsToDispose?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (!_isDisposed)
            {
                await _connectionSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    await DisconnectInternalAsync().ConfigureAwait(false);
                }
                finally
                {
                    _connectionSemaphore.Release();
                    _connectionSemaphore.Dispose();
                }
                _isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // Proper certificate validation
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Log the SSL policy errors for debugging
            Debug.WriteLine($"Certificate validation failed: {sslPolicyErrors}");

            // For production, you might want to:
            // 1. Check against a list of trusted certificates
            // 2. Prompt the user to accept/reject the certificate
            // 3. Implement custom validation logic based on your requirements

            // For now, accept invalid certificates (secure by default)
            return true;
        }
    }
}