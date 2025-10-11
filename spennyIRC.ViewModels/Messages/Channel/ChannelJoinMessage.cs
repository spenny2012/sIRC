using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.Channel;

public class ChannelJoinMessage(IIrcSession session) : MessageBase(session)
{
    public string Nick { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
}