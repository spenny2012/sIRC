using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting;

public class IrcCommand : IIrcCommand
{
    public string Name { get; set; }
    public string Description { get; set; } = "";
    public Func<string, IIrcSession, Task> Command { get; set; }
}