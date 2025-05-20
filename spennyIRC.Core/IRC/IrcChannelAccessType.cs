namespace spennyIRC.Core.IRC;

public enum IrcChannelAccessType // TODO: reconsider redoing this logic
{
    NONE = 0,
    BAN,
    EXEMPT,
    VOICE,
    HOP,
    OP,
    SOP,
    FOUNDER
}
