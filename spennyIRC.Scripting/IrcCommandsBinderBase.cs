using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting
{
    public abstract class IrcCommandsBinderBase(IIrcCommands commands)
    {
        protected void AddCommand(string name, string description, Func<string, IIrcSession, Task> func)
        {
            commands.AddCommand(name, description, new IrcCommand
            {
                Name = name,
                Description = description,
                Command = func,
            });
        }
    }
}