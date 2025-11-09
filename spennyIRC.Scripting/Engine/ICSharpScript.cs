namespace spennyIRC.Scripting.Engine;

public interface ICSharpScript : IDisposable
{
    string Name { get; }
    string Version { get; }
    string Author { get; }
    string Description { get; }

    void Initialize();

    void Shutdown();
}