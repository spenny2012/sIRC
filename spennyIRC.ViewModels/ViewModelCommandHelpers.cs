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
        string? name = parameters.Split(' ').FirstOrDefault();
        if (name == null) return Task.CompletedTask;

        // TODO: strip invalid chars or don't allow
        WeakReferenceMessenger.Default.Send(new OpenQueryMessage(session) { Nick = name });
        return Task.CompletedTask;
    }
}