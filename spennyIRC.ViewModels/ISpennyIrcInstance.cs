using spennyIRC.Core.IRC;
using spennyIRC.Scripting;

namespace spennyIRC.ViewModels;

// TODO: remove this useless wrapper class
public interface ISpennyIrcInstance
{
    IIrcSession Session { get; }
    IIrcCommands IrcCommands { get; }
}
