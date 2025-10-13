using spennyIRC.Scripting;

namespace spennyIRC.ViewModels;

public class UICommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
{
    public void Bind()
    {
        // UI commands
        //AddCommand("list", ViewModelCommandHelpers.ListAsync);
        AddCommand("clear", "clears a chat window", ViewModelCommandHelpers.ClearAsync);
    }
}