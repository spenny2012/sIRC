namespace spennyIRC.Core.IRC;

public interface IIrcSession
{
    string ActiveWindow { get; set; }
    IIrcClient Client { get; }
    IIrcEvents Events { get; }
    IIrcServer Server { get; }
    IIrcLocalUser LocalUser { get; }
    IIrcInternalAddressList Ial { get; }
    IIrcClientManager ClientManager { get; }
    IEchoService EchoService { get; }
}
