namespace spennyIRC.Core.IRC;

public interface IIrcChannel
{
    public string Name { get; set; }
    public string? Topic { get; set; }
    public List<char> Modes { get; set; }
}
