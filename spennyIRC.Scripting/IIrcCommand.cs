using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting;

public interface IIrcCommand
{
    string Name { get; }
    string Description { get; set; }
    Func<string, IIrcSession, Task> Command { get; set; }
}