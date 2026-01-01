namespace spennyIRC.Core.IRC;

public interface IIrcReceivedContext
{
    IIrcSession Session { get; set; }
    string Line { get; set; }
    string[] LineParts { get; set; }
    string Event { get; set; }
    string Location { get; set; }
    string? Trailing { get; set; }
    string? Nick { get; set; }
}