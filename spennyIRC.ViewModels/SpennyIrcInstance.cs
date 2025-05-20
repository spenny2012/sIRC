using Microsoft.Extensions.DependencyInjection;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting;

namespace spennyIRC.ViewModels;

public class SpennyIrcInstance(IIrcSession session, IIrcCommands ircCommands, IServiceScope scope) : ISpennyIrcInstance, IDisposable
{
    public IIrcSession Session => session;

    public IIrcCommands IrcCommands => ircCommands;

    public void Dispose()
    {
        scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}
