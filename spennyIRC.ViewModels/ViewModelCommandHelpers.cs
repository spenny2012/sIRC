using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
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
        IrcCommandParametersInfo parameterInfo = parameters.CreateParameters();
        if (!parameterInfo.HasParameters) return Task.CompletedTask;

        string? name = parameterInfo.GetParam<string?>(0);
        if (name == null) return Task.CompletedTask;

        // TODO: strip invalid chars or don't allow
        WeakReferenceMessenger.Default.Send(new UserOpenedQueryMessage(session) { Nick = name });
        return Task.CompletedTask;
    }
}