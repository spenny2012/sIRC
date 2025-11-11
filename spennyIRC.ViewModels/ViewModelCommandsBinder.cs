using spennyIRC.Scripting.Commands;
using spennyIRC.Scripting.Engine;

namespace spennyIRC.ViewModels;

public class ViewModelCommandsBinder(IIrcCommands commands, ICSharpScriptManager scriptManager) : IrcCommandsBinderBase(commands, scriptManager)
{
    public void Bind()
    {
        // UI commands
        //AddCommand("list", ViewModelCommandHelpers.ListAsync);
        BindFoundCommands();
        commands.AddCommand("clear", "clears a chat window", ViewModelCommandHelpers.ClearAsync);
        commands.AddCommand("query", "opens a new query window", ViewModelCommandHelpers.QueryAsync);
    }
}