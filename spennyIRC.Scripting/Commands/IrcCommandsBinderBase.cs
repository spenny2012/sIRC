using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Attributes;
using System.Reflection;

namespace spennyIRC.Scripting.Commands;

public abstract class IrcCommandsBinderBase(IIrcCommands commands)
{
    protected void AddCommand(string name, string description, Func<string, IIrcSession, Task> func)
    {
        commands.AddCommand(name, description, new IrcCommand
        {
            Name = name,
            Description = description,
            Command = func,
        });
    }

    protected void BindFoundCommands()
    {
        IEnumerable<Type> types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttribute<IrcCommandClassAttribute>() != null);

        foreach (Type? type in types)
        {
            IEnumerable<MethodInfo> methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<IrcCommandAttribute>() != null);

            foreach (MethodInfo? method in methods)
            {
                IrcCommandAttribute? attribute = method.GetCustomAttribute<IrcCommandAttribute>();

                if (attribute == null) continue;

                string? commandName = attribute.Command;

                if (string.IsNullOrEmpty(commandName))
                {
                    commandName = method.Name;
                    if (commandName.EndsWith("async", StringComparison.OrdinalIgnoreCase))
                    {
                        commandName = commandName[..^5];
                    }
                }

                commandName = commandName.ToLower();

                string commandDescription = attribute.Description;

                Func<string, IIrcSession, Task> func = (Func<string, IIrcSession, Task>)Delegate.CreateDelegate(
                    typeof(Func<string, IIrcSession, Task>),
                    method);

                AddCommand(commandName, commandDescription, func);
            }
        }
    }
}