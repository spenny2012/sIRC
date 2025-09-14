using CommunityToolkit.Mvvm.Input;
using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels;

public interface IChatWindow : IDisposable
{
    string Name { get; set; }
    string Caption { get; set; }
    bool IsSelected { get; set; }
    string Text { get; set; }
    IIrcSession Session { get; }
    IAsyncRelayCommand ExecuteCommand { get; }
}

