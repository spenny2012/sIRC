namespace spennyIRC.Core.IRC;

public class IrcSession(IIrcEvents events,
    IIrcClient client,
    IIrcServer server,
    IIrcLocalUser user,
    IIrcInternalAddressList addressList,
    IWindowService echoService) : IIrcSession
{
    private IrcClientManager? _clientManger;

    public string ActiveWindow { get; set; } = "Status";
    public IIrcClient Client => client;
    public IIrcClientManager ClientManager => _clientManger ??= new IrcClientManager(this);
    public IWindowService WindowService => echoService;
    public IIrcEvents Events => events;
    public IIrcInternalAddressList Ial => addressList;
    public IIrcLocalUser LocalUser => user;
    public IIrcServer Server => server;
}