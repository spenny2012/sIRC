namespace spennyIRC.Core.IRC;

public interface IIrcUserChannel
{
    List<IrcChannelAccessType> Access { get; set; }
}