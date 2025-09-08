
namespace spennyIRC.Core.IRC;

public interface IIrcClientManager
{
    Task ConnectAsync(string server, int port, bool useSsl = false);
    Task QuitAsync(string quitMsg = "Test"); // TODO: introduce constant
}