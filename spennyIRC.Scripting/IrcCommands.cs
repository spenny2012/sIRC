using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting;


public class IrcCommands : IIrcCommands
{
    public Dictionary<string, IIrcCommand> Commands { get; private set; } = new (StringComparer.OrdinalIgnoreCase);

    public bool AddCommand(string name, IIrcCommand command)
    {
        if (Commands.TryGetValue(name, out _))
        {
            return false;
        }
        Commands[name] = command;
        return true;
    }

    //public bool AddCommand(string name, IIrcCommand command)
    //{
    //    if (Commands.TryGetValue(name, out _))
    //    {
    //        return false;
    //    }
    //    Commands[name] = command;
    //    return true;
    //}

    public async Task ExecuteCommand(string name, string? parameters, IIrcSession session)
    {
        if (Commands.TryGetValue(name, out IIrcCommand? foundCommand))
        {
            await foundCommand.Command.Invoke(parameters!, session);
        }
    }
}
