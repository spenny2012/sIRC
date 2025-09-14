namespace spennyIRC.Core.IRC;

public interface IIrcChannel
{
    public List<char> Modes { get; set; }
    public string Name { get; set; }
    public string? Topic { get; set; }
}
