using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting.Commands;

public class IrcCommand : IIrcCommand
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required Func<string, IIrcSession, Task> Command { get; set; }
}