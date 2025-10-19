using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Attributes;
using spennyIRC.ViewModels.Messages.Window;

namespace spennyIRC.ViewModels;

[IrcCommandClass("Ui commands")]
public class ViewModelCommandHelpers
{
    [IrcCommand("Clears the text of a window.")]
    public static Task ClearAsync(string parameters, IIrcSession session)
    {
        session.WindowService.Clear(session.ActiveWindow);
        return Task.CompletedTask;
    }

    [IrcCommand("Opens a new query")]
    public static Task QueryAsync(string parameters, IIrcSession session)
    {
        WeakReferenceMessenger.Default.Send(new OpenQueryMessage(session) { Nick = parameters });
        return Task.CompletedTask;
    }
}