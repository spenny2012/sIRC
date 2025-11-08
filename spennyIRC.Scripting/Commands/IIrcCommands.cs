using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting.Commands;

public interface IIrcCommands
{
    Dictionary<string, IIrcCommand> Commands { get; }

    bool AddCommand(string name, string description, IIrcCommand command);
    bool AddCommand(string name, string description, Func<string, IIrcSession, Task> command);
    Task ExecuteCommand(string name, string? parameters, IIrcSession session);
}