using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.Channel;

public class ChannelAddMessage(IIrcSession session) : MessageBase(session)
{
    public string Channel { get; set; } = string.Empty;
}