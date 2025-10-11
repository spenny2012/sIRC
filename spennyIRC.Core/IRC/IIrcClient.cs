namespace spennyIRC.Core.IRC;

public interface IIrcClient : IDisposable
{
    Func<string, Task>? OnDataReceivedHandler { get; set; }
    Func<string, Task>? OnDisconnectedHandler { get; set; }

    Task ConnectAsync(string server, int port, bool useSsl = false);

    Task DisconnectAsync();

    Task SendMessageAsync(string message);
}