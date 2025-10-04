namespace spennyIRC.Core.IRC;

public class IrcUser : IIrcUser
{
    public string Nick { get; set; }
    public string? Ident { get; set; }
    public string? Domain { get; set; }

    public Dictionary<string, IIrcUserChannel> Channels { get; set; } = [];
}