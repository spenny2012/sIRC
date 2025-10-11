namespace spennyIRC.Core.IRC;

public class IrcUserChannel : IIrcUserChannel
{
    public List<IrcChannelAccessType> Access { get; set; } = [];
}