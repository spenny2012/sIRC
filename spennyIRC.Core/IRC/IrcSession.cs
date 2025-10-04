namespace spennyIRC.Core.IRC;

public class IrcSession(IIrcEvents events,
    IIrcClient client,
    IIrcServer server,
    IIrcLocalUser user,
    IIrcInternalAddressList addressList,
    IIrcClientManager ircClientManager,
    IWindowService echoService) : IIrcSession
{
    public string ActiveWindow { get; set; } = "Status";
    public IIrcClient Client => client;
    public IIrcClientManager ClientManager => ircClientManager;
    public IWindowService EchoService => echoService;
    public IIrcEvents Events => events;
    public IIrcInternalAddressList Ial => addressList;
    public IIrcLocalUser LocalUser => user;
    public IIrcServer Server => server;
}