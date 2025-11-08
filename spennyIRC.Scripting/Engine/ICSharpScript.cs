namespace spennyIRC.Scripting.Engine;

public interface ICSharpScript
{
    string Name { get; }
    string Version { get; }

    void Execute();

    void Initialize();

    void Shutdown();
}