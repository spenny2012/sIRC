using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Engine;

namespace spennyIRC.Scripting.Commands;

public class IrcCommandsBinder(IIrcCommands commands, ICSharpScriptManager scriptManager) : IrcCommandsBinderBase(commands, scriptManager)
{
    public void Bind()
    {
        commands.AddCommand("help", "lists commands", ListCommandsAsync);
        commands.AddCommand("load", "loads a C# script", LoadScriptAsync);

        BindFoundCommands();
    }

    private Task LoadScriptAsync(string parameters, IIrcSession session)
    {
        if (!Path.Exists(parameters))
        {
            session.WindowService.Echo(session.ActiveWindow, $"*** Invalid path '{parameters}'");
            return Task.CompletedTask;
        }

        string fullScriptPath = Path.GetFullPath(parameters);

        try
        {
            ICSharpScript? script = scriptManager.ExecuteScript<ICSharpScript>(parameters);
            script?.Initialize();

            session.WindowService.Echo(session.ActiveWindow, $"*** Loaded script '{script!.Name}' ({fullScriptPath})");
        }
        catch (Exception ex)
        {
            session.WindowService.Echo(session.ActiveWindow, $"*** Error loading script '({fullScriptPath})': {ex}");
        }

        return Task.CompletedTask;
    }

    private Task ListCommandsAsync(string parameters, IIrcSession session)
    {
        session.WindowService.Echo(session.ActiveWindow, "-\r\nList of commands:");

        foreach (KeyValuePair<string, IIrcCommand> command in commands.Commands)
        {
            session.WindowService.Echo(session.ActiveWindow, $"* {command.Value.Name} - {command.Value.Description}");
        }

        session.WindowService.Echo(session.ActiveWindow, "-");

        return Task.CompletedTask;
    }
}