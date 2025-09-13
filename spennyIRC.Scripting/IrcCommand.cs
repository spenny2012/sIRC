using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting;

public class IrcCommand : IIrcCommand
{
    public required string Name { get; set; }
    public string Info { get; set; } = "There is no information available for this command.";
    public required Func<string, IIrcSession, Task> Command { get; set; }
}
