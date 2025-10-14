using spennyIRC.Core.IRC;
using System.Reflection;

namespace spennyIRC.Scripting.Helpers
{
    internal static class PrintPropertiesHelper
    {
        public static void BasicPrintProperties<T>(T obj, IIrcSession session)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            Type type = obj.GetType();
            IOrderedEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead)
                                 .OrderBy(p => p.Name);

            foreach (PropertyInfo? property in properties)
            {
                try
                {
                    object? value = property.GetValue(obj);
                    session.EchoService.DoEcho(session.ActiveWindow, $"{property.Name}: {value?.ToString() ?? "null"}");
                }
                catch (Exception ex)
                {
                    session.EchoService.DoEcho(session.ActiveWindow, $"{property.Name}: [Error: {ex.Message}]");
                }
            }
        }

        public static void PrintProperties(object obj, IIrcSession session)
        {
            Type type = obj.GetType();
            session.EchoService.Echo(session.ActiveWindow, $"Properties of {type.Name}:");

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                try
                {
                    object? value = property.GetValue(obj);
                    string? valueString = value != null ? value.ToString() : "null";
                    session.EchoService.Echo(session.ActiveWindow, $"{property.Name}: {valueString}");
                }
                catch (Exception ex)
                {
                    session.EchoService.Echo(session.ActiveWindow, $"{property.Name}: Error retrieving value ({ex.Message})");
                }
            }
        }
    }
}