using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting.Commands;

public class IrcCommands : IIrcCommands
{
    public Dictionary<string, IIrcCommand> Commands { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool AddCommand(string name, string description, IIrcCommand command)
    {
        if (Commands.TryGetValue(name, out _))
        {
            return false;
        }
        else
        {
            Commands[name] = command;

            return true;
        }
    }

    public bool AddCommand(string name, string description, Func<string, IIrcSession, Task> command)
    {
        if (Commands.TryGetValue(name, out _))
        {
            return false;
        }

        Commands[name] = new IrcCommand
        {
            Name = name,
            Command = command,
            Description = description
        };

        return true;
    }

    public async Task ExecuteCommand(string name, string? parameters, IIrcSession session)
    {
        if (Commands.TryGetValue(name, out IIrcCommand? foundCommand))
        {
            await foundCommand.Command.Invoke(parameters!, session);
        }
    }
}
