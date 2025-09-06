namespace spennyIRC.Core.IRC;

public interface IIrcClient : IDisposable
{
    Func<string, Task>? OnDataReceivedHandler { get; set; }
    Func<string, Task>? OnDisconnectedHandler { get; set; }
    Task ConnectAsync(string server, string port);
    Task DisconnectAsync();
    Task SendMessageAsync(string message);
}