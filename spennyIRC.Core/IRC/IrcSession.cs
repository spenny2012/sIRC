namespace spennyIRC.Core.IRC;

public class IrcSession(IIrcEvents events,
    IIrcClient client,
    IIrcServer server,
    IIrcLocalUser user,
    IIrcInternalAddressList addressList,
    IIrcClientManager ircClientManager,
    IEchoService echoService) : IIrcSession
{
    public string ActiveWindow { get; set; } = "Status";
    public IIrcEvents Events => events;
    public IIrcClient Client => client;
    public IIrcServer Server => server;
    public IIrcLocalUser LocalUser => user;
    public IIrcInternalAddressList Ial => addressList;
    public IIrcClientManager ClientManager => ircClientManager;
    public IEchoService EchoService => echoService;
}
