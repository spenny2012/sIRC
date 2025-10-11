namespace spennyIRC.Scripting.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class IrcCommandAttribute(string description) : Attribute
    {
        public string? Command { get; set; }
        public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

        public IrcCommandAttribute(string command, string description) : this(description)
        {
            Command = command;
        }
    }
}