using spennyIRC.Scripting.Commands;

namespace spennyIRC.ViewModels;

public class ViewModelCommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
{
    public void Bind()
    {
        // UI commands
        //AddCommand("list", ViewModelCommandHelpers.ListAsync);
        BindFoundCommands();
        AddCommand("clear", "clears a chat window", ViewModelCommandHelpers.ClearAsync);
        AddCommand("query", "opens a new query window", ViewModelCommandHelpers.QueryAsync);
    }
}