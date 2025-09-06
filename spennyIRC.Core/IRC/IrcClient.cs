using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace spennyIRC.Core.IRC
{
    public class IrcClient : IIrcClient, IDisposable
    {
        private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

        private TcpClient? _tcpClient;
        private Stream? _networkStream;
        private bool _isDisposed;
        private CancellationTokenSource? _cts;

        public Func<string, Task>? OnDataReceivedHandler { get; set; }
        public Func<string, Task>? OnDisconnectedHandler { get; set; }

        public async Task ConnectAsync(string server, string port)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            await _connectionSemaphore.WaitAsync();
            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                    throw new InvalidOperationException("Already connected to a server.");

                bool useSsl = port.StartsWith('+');
                var portSpan = useSsl ? port[1..] : port;
                if (!int.TryParse(portSpan, out int actualPort))
                    throw new ArgumentException("Invalid port format.", nameof(port));

                await DisconnectInternalAsync();

                _tcpClient = new TcpClient();
                try
                {
                    await _tcpClient.ConnectAsync(server, actualPort);
                    _networkStream = _tcpClient.GetStream();

                    if (useSsl)
                    {
                        SslStream sslStream = new(_networkStream, false, ValidateServerCertificate, null);
                        try
                        {
                            await sslStream.AuthenticateAsClientAsync(server);
                            _networkStream = sslStream;
                        }
                        catch
                        {
                            await sslStream.DisposeAsync();
                            throw;
                        }
                    }
                }
                catch
                {
                    _tcpClient?.Dispose();
                    _tcpClient = null;
                    _networkStream?.Dispose();
                    _networkStream = null;
                    throw;
                }

                _cts = new CancellationTokenSource();
                _ = Task.Run(() => StartReceivingMessagesAsync(_cts.Token), _cts.Token);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        public async Task SendMessageAsync(string message)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            await _connectionSemaphore.WaitAsync();
            try
            {
                if (_networkStream == null)
                    throw new InvalidOperationException("Not connected to the server.");

                byte[] messageBytes = Encoding.UTF8.GetBytes(message + "\r\n");
                await _networkStream.WriteAsync(messageBytes);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        private async Task StartReceivingMessagesAsync(CancellationToken cancellationToken)
        {
            if (_networkStream == null)
                return;

            using (StreamReader reader = new(_networkStream, Encoding.UTF8, false, 2048, leaveOpen: true))
            {
                while (_tcpClient != null && _tcpClient.Connected && !_isDisposed && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        string? line = await reader.ReadLineAsync(cancellationToken);
                        if (line == null)
                            break;

                        if (OnDataReceivedHandler != null)
                            _ = OnDataReceivedHandler(line);  // Fire-and-forget to avoid blocking the loop
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error receiving message: {e.Message}");
                        break;
                    }
                }
            }

            try
            {
                if (OnDisconnectedHandler != null)
                    await OnDisconnectedHandler("Disconnected from the server.");
            }
            finally
            {
                await DisconnectInternalAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            await _connectionSemaphore.WaitAsync();
            try
            {
                await DisconnectInternalAsync();
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        public async Task DisconnectInternalAsync()
        {
            _cts?.Cancel();
            if (_networkStream != null)
            {
                try
                {
                    await _networkStream.DisposeAsync();
                }
                catch { }  // Ignore disposal errors
            }
            _tcpClient?.Dispose();

            _tcpClient = null;
            _networkStream = null;
            _cts = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _connectionSemaphore.Wait();
                    try
                    {
                        DisconnectInternalAsync().GetAwaiter().GetResult();  
                        _cts?.Dispose();
                    }
                    finally
                    {
                        _connectionSemaphore.Release();
                    }
                }
                _isDisposed = true;
            }
        }

        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // TODO: trigger prompt user
            return true;
        }
    }
}