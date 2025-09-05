using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting
{
    public abstract class IrcCommandsBinderBase(IIrcCommands commands)
    {
        protected void AddCommand(string name, Func<string, IIrcSession, Task> func)
        {
            commands.AddCommand(name, new IrcCommand
            {
                Name = name,
                Command = func,
            });
        }
    }
}
