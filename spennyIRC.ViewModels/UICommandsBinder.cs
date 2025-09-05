using spennyIRC.Scripting;
using spennyIRC.Scripting.Helpers;

namespace spennyIRC.ViewModels
{
    public class UICommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
    {
        public void Bind()
        {
            // UI commands
            AddCommand("list", IrcCommandHelpers.ListAsync);
            AddCommand("clear", IrcCommandHelpers.ClearAsync);
        }
    }
}
