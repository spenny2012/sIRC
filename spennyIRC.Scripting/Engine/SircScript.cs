using spennyIRC.Core.IRC;

namespace spennyIRC.Scripting.Engine
{
    public abstract class SircScript : ICSharpScript 
    {
        //public readonly Dictionary<string, Func<IIrcReceivedContext, Task>>
        protected SircScript()
        {
        }

        public abstract string Name { get; }
        public virtual string Version { get; } = "1.0";
        public virtual string Author { get; } = "Unspecified";
        public virtual string Description { get; } = string.Empty;

        public abstract void Execute();
        public abstract void Initialize();
        public abstract void Shutdown();
        public abstract void Dispose();

        public virtual void TriggerEvent(IIrcReceivedContext ctx)
        {
            var evt  = ctx.Event;
            switch (evt)
            {
            }
        }
    }
}
