using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.Server;

public class ServerDisconnectedMessage(IIrcSession session) : MessageBase(session)
{
}