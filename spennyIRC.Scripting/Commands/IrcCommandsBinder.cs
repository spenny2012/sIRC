using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting.Commands;

//TODO: add help command
public class IrcCommandsBinder(IIrcCommands _commands) : IrcCommandsBinderBase(_commands)
{
    public void Bind()
    {
        AddCommand("help", "lists commands", ListCommandsAsync);

        BindFoundCommands();
    }

    private async Task ListCommandsAsync(string parameters, IIrcSession session)
    {
        session.WindowService.Echo(session.ActiveWindow, "-\r\nList of commands:");

        foreach (KeyValuePair<string, IIrcCommand> command in _commands.Commands)
        {
            session.WindowService.Echo(session.ActiveWindow, $"* {command.Value.Name} - {command.Value.Description}");
        }

        session.WindowService.Echo(session.ActiveWindow, "-");
    }
}