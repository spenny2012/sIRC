namespace spennyIRC.Core.IRC;

public interface IIrcLocalUser
{
    string Ident { get; set; }
    string Nick { get; set; }
    string Nick2 { get; set; }
    string Realname { get; set; }
    bool Away { get; set; }
    public Dictionary<string, IIrcChannel> Channels { get; set; }
}