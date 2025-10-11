namespace spennyIRC.Core.IRC;

public class IrcLocalUser : IIrcLocalUser
{
    public string Nick { get; set; } = string.Empty;
    public string Nick2 { get; set; } = string.Empty;
    public string Ident { get; set; } = string.Empty;
    public string Realname { get; set; } = string.Empty;
    public bool Away { get; set; }
    public Dictionary<string, IIrcChannel> Channels { get; set; } = [];
}