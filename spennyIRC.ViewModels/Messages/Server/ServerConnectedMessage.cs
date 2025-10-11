using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.Server;

public class ServerConnectedMessage(IIrcSession session) : MessageBase(session)
{
}