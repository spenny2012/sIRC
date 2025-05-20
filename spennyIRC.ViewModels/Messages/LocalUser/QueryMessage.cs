namespace spennyIRC.ViewModels.Messages.LocalUser;

public class QueryMessage(ISpennyIrcInstance session) : MessageBase(session)
{
    public string Nick { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
