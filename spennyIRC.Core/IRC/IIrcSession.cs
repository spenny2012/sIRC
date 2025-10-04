namespace spennyIRC.Core.IRC;

public interface IIrcSession
{
    string ActiveWindow { get; set; }
    IIrcClient Client { get; }
    IIrcClientManager ClientManager { get; }
    IWindowService EchoService { get; }
    IIrcEvents Events { get; }
    IIrcInternalAddressList Ial { get; }
    IIrcLocalUser LocalUser { get; }
    IIrcServer Server { get; }
}
