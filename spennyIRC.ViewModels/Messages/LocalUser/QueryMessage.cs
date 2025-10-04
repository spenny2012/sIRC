using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.LocalUser;

public class QueryMessage(IIrcSession session) : MessageBase(session)
{
    public string Nick { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}