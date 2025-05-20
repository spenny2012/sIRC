namespace spennyIRC.ViewModels.Messages.Channel;

public class ChannelAddMessage(ISpennyIrcInstance session) : MessageBase(session)
{
    public string Channel { get; set; } = string.Empty;
}
