using spennyIRC.Scripting.Engine;

public class HelloWorldScript : ICSharpScript
{
    public string Name => "Hello World Script";
    public string Version => "1.0";
    public string Author => "SK";
    public string Description => "A simple test script.";

    public void Initialize()
    {
    }

    public void Execute()
    {
    }

    public void Shutdown()
    {
    }
}