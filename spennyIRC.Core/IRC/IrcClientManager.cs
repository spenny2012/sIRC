namespace spennyIRC.Core.IRC;

//TODO: 2) Add IIrcSession
public class IrcClientManager : IIrcClientManager, IDisposable
{
    private readonly IIrcLocalUser _user;
    private readonly IIrcClient _ircClient;
    private readonly IIrcEvents _ircClientEvents;
    private bool isDisposed;

    public IrcClientManager(IIrcClient client, IIrcEvents events, IIrcLocalUser user)
    {
        _user = user;
        _ircClient = client;
        _ircClientEvents = events;
        _ircClient.OnDataReceivedHandler += OnMessageReceived;
        _ircClient.OnDisconnectedHandler += OnDisconnected;
    }

    public async Task ConnectAsync(string server, int port, bool useSsl = false)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        await _ircClient.ConnectAsync(server, port, useSsl);

        await Task.Delay(TimeSpan.FromSeconds(1));

        await _ircClient.SendMessageAsync($"NICK {_user.Nick}");
        await _ircClient.SendMessageAsync($"USER {_user.Ident} * * :{_user.Realname}");
    }

    public async Task QuitAsync(string quitMsg = "Test")
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        await _ircClient.SendMessageAsync($"QUIT :{quitMsg}");
        await _ircClient.DisconnectAsync();
    }

    private async Task OnDisconnected(string message)
    {
        IIrcReceivedContext ircContext = IrcReceivedContextFactory.CreateDisconnect(_ircClient, message);
        await _ircClientEvents.TryExecute(ircContext.Event, ircContext);
    }

    private async Task OnMessageReceived(string message)
    {
        string[] lineParts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        /* Handle ping */
        if (string.Equals(lineParts[0], "PING", StringComparison.Ordinal))
        {
            await _ircClient.SendMessageAsync($"PONG {lineParts[1][1..]}");
            return;
        }

        IIrcReceivedContext ircContext = IrcReceivedContextFactory.Create(_ircClient, message, lineParts);
        await _ircClientEvents.TryExecute(ircContext.Event, ircContext);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                _ircClient.Dispose();
            }
            isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}