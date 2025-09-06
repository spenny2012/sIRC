using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels
{
    public class ViewModelCommandHelpers
    {
        public static Task ClearAsync(string arg1, IIrcSession session)
        {
            //WeakReferenceMessenger.Default.Send(new ChannelAddMessage(session));
            return Task.CompletedTask;
        }
    }
}
