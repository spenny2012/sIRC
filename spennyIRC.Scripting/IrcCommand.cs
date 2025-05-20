using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting;

public class IrcCommand : IIrcCommand
{
    public string Name { get; set; } = string.Empty;
    public string Info { get; set; } = "There is no information for this command.";
    public Func<string, IIrcSession, Task> Command { get; set; }
}
