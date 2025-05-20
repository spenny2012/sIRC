using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting;

public interface IIrcCommand
{
    string Name { get; set; }
    string Info { get; set; }
    Func<string, IIrcSession, Task> Command { get; set; }
}
