using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Attributes;
using System.Reflection;

namespace spennyIRC.Scripting
{
    //TODO: add help command
    public class IrcCommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
    {
        public void Bind()
        {
            AddCommand("help", "lists commands", ListCommandsAsync);

            BindFoundCommands();
        }

        private async Task ListCommandsAsync(string parameters, IIrcSession session)
        {
        }
    }
}