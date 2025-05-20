using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting;

public interface IIrcCommands
{
    Dictionary<string, IIrcCommand> Commands { get; set; }
    bool AddCommand(string name, IIrcCommand command);
    Task ExecuteCommand(string name, string? parameters, IIrcSession session);
}
