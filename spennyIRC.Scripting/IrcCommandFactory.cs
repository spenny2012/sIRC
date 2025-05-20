using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting;

// TODO: is this really necessary?
public class IrcCommandFactory
{
    public IIrcCommand CreateCommand(string name, Func<string, IIrcSession, Task> func) => new IrcCommand
    {
        Name = name,
        Command = func
    };

    public IIrcCommand CreateCommand(string name, string info, Func<string, IIrcSession, Task> func)
    {
        return new IrcCommand
        {
            Name = name,
            Info = info,
            Command = func
        };
    }
}
