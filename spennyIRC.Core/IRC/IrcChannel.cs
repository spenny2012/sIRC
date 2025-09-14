namespace spennyIRC.Core.IRC;

public class IrcChannel : IIrcChannel
{
    public List<char> Modes { get; set; } = [];
    public string Name { get; set; } = string.Empty;
    public string? Topic { get; set; }
}
