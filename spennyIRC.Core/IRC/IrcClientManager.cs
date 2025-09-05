using System.Diagnostics;
namespace spennyIRC.Core.IRC;

//TODO: 2) Add IIrcSession
public class IrcClientManager : IIrcClientManager, IDisposable
{
    private static readonly IrcReceivedContextFactory _ctxFactory = new();
    private readonly IIrcLocalUser _user;
    private readonly IIrcClient _ircClient;
    private readonly IIrcEvents _ircClientEvents;
    private bool isDisposed;

    public IrcClientManager(IIrcClient client, IIrcEvents events, IIrcLocalUser user)
    {
        _user = user;
        _ircClient = client;
        _ircClientEvents = events;
        _ircClient.OnMessageReceived += OnMessageReceived;
        _ircClient.OnDisconnected += OnDisconnected;
    }

    public async Task ConnectAsync(string server, string port)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        await _ircClient.ConnectAsync(server, port);
        await _ircClient.SendMessageAsync($"NICK {_user.Nick}");
        await _ircClient.SendMessageAsync($"USER {_user.Ident} YourHost YourServer :{_user.Realname}");
    }

    private async Task OnDisconnected(string message)
    {
        IIrcReceivedContext ircContext = _ctxFactory.CreateDisconnect(_ircClient, message);

        await _ircClientEvents.TryExecute(ircContext.Event, ircContext);
    }

    private async Task OnMessageReceived(string message)
    {
#if DEBUG
        Debug.WriteLine(message);
#endif
        string[] lineParts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        /* Handle ping */
        if (string.Equals(lineParts[0], "PING", StringComparison.Ordinal))
        {
            await _ircClient.SendMessageAsync($"PONG {lineParts[1][1..]}");
            return;
        }

        IIrcReceivedContext ircContext = _ctxFactory.Create(_ircClient, message, lineParts);

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