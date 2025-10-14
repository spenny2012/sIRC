namespace spennyIRC.Core.IRC;

public interface IIrcUser
{
    string Nick { get; set; }
    string? Ident { get; set; }
    string? Domain { get; set; }
    Dictionary<string, IIrcUserChannel> Channels { get; set; }
}