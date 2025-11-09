namespace spennyIRC.Core.IRC;

public record struct IrcReceivedContext : IIrcReceivedContext
{
    public IIrcSession Session { get; set; }
    public string Line { get; set; }
    public string[] LineParts { get; set; }
    public string Event { get; set; }
    public string Recipient { get; set; }
    public string? Trailing { get; set; }
    public string? Nick { get; set; }
}