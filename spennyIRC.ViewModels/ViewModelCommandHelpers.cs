using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Attributes;

namespace spennyIRC.ViewModels;

[IrcCommandClass("Ui commands")]
public class ViewModelCommandHelpers
{
    [IrcCommand("Clears the text of a window.")]
    public static Task ClearAsync(string arg1, IIrcSession session)
    {
        //WeakReferenceMessenger.Default.Send(new ChannelAddMessage(session));
        session.WindowService.Clear(session.ActiveWindow);
        return Task.CompletedTask;
    }
}