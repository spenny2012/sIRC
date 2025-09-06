using spennyIRC.Core.IRC;
using System.Windows.Input;

namespace spennyIRC.ViewModels;

public interface IChatWindow : IDisposable
{
    string Name { get; set; }
    string Caption { get; set; }
    bool IsSelected { get; set; }
    string Text { get; set; }
    IIrcSession Session { get; }
    ICommand ExecuteCommand { get; }
}


