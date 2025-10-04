using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.Channel;

public class ChannelTopicMessage(IIrcSession session) : MessageBase(session)
{
    public string Channel { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}