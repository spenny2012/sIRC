namespace spennyIRC.Core.IRC;

public interface IIrcClient : IDisposable
{
    Func<string, Task>? OnMessageReceived { get; set; }
    Func<string, Task>? OnDisconnected { get; set; }

    Task ConnectAsync(string server, string port);
    void Disconnect();
    Task SendMessageAsync(string message);
}