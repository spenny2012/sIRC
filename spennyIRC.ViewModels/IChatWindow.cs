using System.Windows.Input;

namespace spennyIRC.ViewModels;

public interface IChatWindow
{
    string Name { get; set; }
    string Caption { get; set; }
    bool IsSelected { get; set; }
    string Text { get; set; }
    ISpennyIrcInstance Session { get; set; }
    ICommand ExecuteCommand { get; }
}


