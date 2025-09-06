using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages;

public abstract class MessageBase(IIrcSession session)
{
    public IIrcSession Session => session;
}