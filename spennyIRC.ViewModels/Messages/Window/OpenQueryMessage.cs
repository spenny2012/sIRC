using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.Window;

public class OpenQueryMessage(IIrcSession session) : MessageBase(session)
{
    public string Nick { get; set; }
}