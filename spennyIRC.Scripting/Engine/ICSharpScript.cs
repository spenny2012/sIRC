using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting.Engine;

public interface ICSharpScript
{
    string Name { get; }
    string Version { get; }
    string Author { get; }
    string Description { get; }

    void Initialize();
    void Shutdown();
    Task TriggerEvents(string eventName, IIrcReceivedContext context);
}