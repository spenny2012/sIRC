namespace spennyIRC.Core.IRC;

//TODO: 2) Add IIrcSession
public class IrcClientManager : IIrcClientManager, IDisposable
{
    private readonly IIrcClient _ircClient;
    private readonly IIrcEvents _ircClientEvents;
    private readonly IIrcLocalUser _user;
    private readonly IIrcSession _session;
    private bool isDisposed;

    public IrcClientManager(IIrcSession session)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        _session = session;
        _user = session.LocalUser;
        _ircClient = session.Client;
        _ircClientEvents = session.Events;

        _ircClient.OnDataReceivedHandler += OnMessageReceived;
        _ircClient.OnDisconnectedHandler += OnDisconnected;
    }

    public async Task ConnectAsync(string server, int port, bool useSsl = false)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        await _ircClient.ConnectAsync(server, port, useSsl);
        await _ircClient.SendMessageAsync($"NICK {_user.Nick}");
        await _ircClient.SendMessageAsync($"USER {_user.Ident} * * :{_user.Realname}");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task QuitAsync(string quitMsg = "Test")
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        await _ircClient.SendMessageAsync($"QUIT :{quitMsg}");
        await _ircClient.DisconnectAsync();

        await Task.Delay(TimeSpan.FromSeconds(1));
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

    private async Task OnDisconnected(string message)
    {
        IIrcReceivedContext ircContext = IrcReceivedContextFactory.CreateDisconnect(_session, message);
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

        IIrcReceivedContext ircContext = IrcReceivedContextFactory.Create(_session, message, lineParts);
        await _ircClientEvents.TryExecute(ircContext.Event, ircContext);
    }
}