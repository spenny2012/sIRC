using Microsoft.CodeAnalysis.CSharp.Syntax;
using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting.Commands;

public class IrcCommands : IIrcCommands
{
    public Dictionary<string, IIrcCommand> Commands { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

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
    public bool RemoveCommand(string name)
    {
        if (Commands.TryGetValue(name, out _))
        {
            Commands.Remove(name);
            return true;
        }

        return false;
    }

    public Task ExecuteCommand(string name, string? parameters, IIrcSession session)
    {
        if (Commands.TryGetValue(name, out IIrcCommand? foundCommand))
        {
            return foundCommand.Command(parameters!, session);
        }

        return Task.CompletedTask;
    }
}
