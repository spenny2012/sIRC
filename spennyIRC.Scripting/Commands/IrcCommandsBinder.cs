using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Engine;

namespace spennyIRC.Scripting.Commands;

public class IrcCommandsBinder(IIrcCommands commands, ICSharpScriptManager scriptManager) : IrcCommandsBinderBase(commands, scriptManager)
{
    public void Bind()
    {
        commands.AddCommand("help", "lists commands", ListCommandsAsync);
        commands.AddCommand("load", "", LoadScriptAsync);

        BindFoundCommands();
    }

    private async Task LoadScriptAsync(string arg1, IIrcSession session)
    {
        if (!Path.Exists(arg1))
        {
            session.WindowService.Echo(session.ActiveWindow, $"*** Invalid path '{arg1}'");
            return;
        }
        var script = scriptManager.ExecuteScript<ICSharpScript>(arg1);
        script?.Initialize();

        session.WindowService.Echo(session.ActiveWindow, $"*** Loaded script '{script.Name}' ({arg1})");
    }

    private async Task ListCommandsAsync(string parameters, IIrcSession session)
    {
        session.WindowService.Echo(session.ActiveWindow, "-\r\nList of commands:");

        foreach (KeyValuePair<string, IIrcCommand> command in commands.Commands)
        {
            session.WindowService.Echo(session.ActiveWindow, $"* {command.Value.Name} - {command.Value.Description}");
        }

        session.WindowService.Echo(session.ActiveWindow, "-");
    }
}