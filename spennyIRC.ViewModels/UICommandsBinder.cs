using spennyIRC.Core.IRC;
using spennyIRC.Scripting;

namespace spennyIRC.ViewModels
{
    public class UICommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
    {
        public void Bind()
        {
            // UI commands
            //AddCommand("list", ViewModelCommandHelpers.ListAsync);
            AddCommand("clear", ViewModelCommandHelpers.ClearAsync);
        }
    }

    public class ViewModelCommandHelpers
    {
        public static Task ClearAsync(string arg1, IIrcSession session)
        {
            //WeakReferenceMessenger.Default.Send(new ChannelAddMessage(session));
            return Task.CompletedTask;
        }
    }
}
