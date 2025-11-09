namespace spennyIRC.Scripting.Engine
{
    public abstract class SircScript : ICSharpScript, IDisposable
    {
        public abstract string Name { get; }
        public virtual string Version { get; } = "1.0";
        public virtual string Author { get; } = "Unspecified";
        public virtual string Description { get; } = string.Empty;

        public abstract void Execute();
        public abstract void Initialize();
        public abstract void Shutdown();
        public abstract void Dispose();
    }
}
