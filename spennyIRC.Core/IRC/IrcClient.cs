using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace spennyIRC.Core.IRC;

public class IrcClient : IIrcClient
{
    private TcpClient? _tcpClient;
    private Stream? _networkStream;
    private bool _isDisposed;
    private CancellationTokenSource? _cts;
    private readonly object _connectionLock = new();

    public Func<string, Task>? OnMessageReceived { get; set; }
    public Func<string, Task>? OnDisconnected { get; set; }

    public async Task ConnectAsync(string server, string port)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        lock (_connectionLock)
        {
            if (_tcpClient != null && _tcpClient.Connected)
                throw new InvalidOperationException("Already connected to a server.");
        }

        bool useSsl = port.StartsWith('+');
        int actualPort = useSsl ? int.Parse(port.AsSpan(1)) : int.Parse(port);

        Disconnect();

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
            _tcpClient.Dispose();
            throw;
        }

        _cts = new CancellationTokenSource();
        _ = Task.Run(() => StartReceivingMessagesAsync(_cts.Token), _cts.Token);
    }

    public async Task SendMessageAsync(string message)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_networkStream == null)
            throw new InvalidOperationException("Not connected to the server.");

        byte[] messageBytes = Encoding.UTF8.GetBytes(message + "\r\n");
        await _networkStream.WriteAsync(messageBytes);
    }

    private async Task StartReceivingMessagesAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_networkStream == null)
            throw new InvalidOperationException("Not connected to the server.");

        using (StreamReader reader = new(_networkStream, Encoding.UTF8, false, 2048, leaveOpen: true))
        {
            while (_tcpClient != null && _tcpClient.Connected && !_isDisposed && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    string? line = await reader.ReadLineAsync(cancellationToken) ?? throw new Exception("Connection closed by the server");

                    if (OnMessageReceived != null)
                        await OnMessageReceived(line);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error receiving message: {e.Message}");
                    break;
                }
            }
        }

        if (OnDisconnected != null)
            await OnDisconnected("Disconnected from the server.");

        Disconnect();
    }

    public void Disconnect()
    {
        lock (_connectionLock)
        {
            _cts?.Cancel();
            _networkStream?.Dispose();
            _tcpClient?.Dispose();

            _tcpClient = null;
            _networkStream = null;
            _cts = null;
        }
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
                Disconnect();
                _cts?.Dispose();
            }
            _isDisposed = true;
        }
    }

    private static bool ValidateServerCertificate(
        object sender,
        System.Security.Cryptography.X509Certificates.X509Certificate? certificate,
        System.Security.Cryptography.X509Certificates.X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // TODO: trigger prompt user
        return true;
    }
}