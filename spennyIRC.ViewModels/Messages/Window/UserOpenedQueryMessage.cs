using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.Window;

public class UserOpenedQueryMessage(IIrcSession session) : MessageBase(session)
{
    public string Nick { get; set; }
}