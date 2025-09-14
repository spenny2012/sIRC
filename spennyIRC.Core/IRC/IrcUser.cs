namespace spennyIRC.Core.IRC;

public class IrcUser : IIrcUser
{
    public required string Nick { get; set; }
    public string? Ident { get; set; }
    public string? Domain { get; set; }
    public Dictionary<string, IIrcUserChannel> Channels { get; set; } = [];
}
