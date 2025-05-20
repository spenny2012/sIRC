namespace spennyIRC.ViewModels.Messages;

public class UserQuitMessage(ISpennyIrcInstance session) : MessageBase(session)
{
    public string Nick { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
