namespace spennyIRC.Core.IRC;

public interface IIrcUserChannel
{
    public List<IrcChannelAccessType> Access { get; set; }
}
