namespace spennyIRC.ViewModels.Messages.Channel;

public class ChannelAddNicksMessage(ISpennyIrcInstance server) : MessageBase(server)
{
    public string Channel { get; set; } = string.Empty;
    public string[] Nicks { get; set; }
}
