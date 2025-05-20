namespace spennyIRC.Core.IRC;

public interface IIrcReceivedContext
{
    IIrcClient IrcClient { get; set; }
    string Line { get; set; }
    string[] LineParts { get; set; }
    string Event { get; set; }
    string Recipient { get; set; }
    string? Trailing { get; set; }
    string? Nick { get; set; }
}