using CommunityToolkit.Mvvm.Input;
using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels;

public interface IChatWindow : IDisposable
{
    string Caption { get; set; }
    IAsyncRelayCommand ClearCommand { get; }
    IAsyncRelayCommand ExecuteCommand { get; }
    bool IsSelected { get; set; }
    string Name { get; set; }
    IIrcSession Session { get; }
    string Text { get; set; }
}