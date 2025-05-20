namespace spennyIRC.ViewModels.Messages.Channel;

public class ChannelTopicChangeMessage(ISpennyIrcInstance session) : MessageBase(session)
{
    public string Channel { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}
