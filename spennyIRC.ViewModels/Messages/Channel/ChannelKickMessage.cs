namespace spennyIRC.ViewModels.Messages.Channel;

public class ChannelKickMessage(ISpennyIrcInstance session) : MessageBase(session)
{
    public string Nick { get; set; } = string.Empty;
    public string KickedNick { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
