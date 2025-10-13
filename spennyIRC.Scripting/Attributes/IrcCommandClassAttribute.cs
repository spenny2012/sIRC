namespace spennyIRC.Scripting.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class IrcCommandClassAttribute(string description) : Attribute
{
    public string Description { get; set; } = description;
}