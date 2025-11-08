namespace spennyIRC.Scripting.Engine;

// Plugin interface that all scripts must implement
public interface ICSharpScript
{
    string Name { get; }
    string Version { get; }

    void Execute();
    void Initialize();
    void Shutdown();
}