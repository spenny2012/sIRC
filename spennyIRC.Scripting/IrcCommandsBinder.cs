using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Attributes;
using spennyIRC.Scripting.Helpers;
using System.Reflection;

namespace spennyIRC.Scripting
{
    public class IrcCommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
    {
        public void Bind()
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

                    string commandName = attribute.Command;

                    if (string.IsNullOrEmpty(commandName))
                    {
                        commandName = method.Name;
                        if (commandName.EndsWith("async", StringComparison.OrdinalIgnoreCase))
                        {
                            commandName = commandName[..^5];
                        }
                    }

                    commandName = commandName.ToLower();

                    Func<string, IIrcSession, Task> func = (Func<string, IIrcSession, Task>)Delegate.CreateDelegate(
                        typeof(Func<string, IIrcSession, Task>),
                        method);

                    AddCommand(commandName, func);
                }
            }
        }
    }
}