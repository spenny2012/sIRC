
namespace spennyIRC.Core.IRC;

public interface IIrcClientManager
{
    Task ConnectAsync(string server, string port);
}