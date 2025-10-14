namespace spennyIRC.Core.IRC;

public interface IIrcChannel
{
    List<char> Modes { get; set; }
    string Name { get; set; }
    string? Topic { get; set; }
}