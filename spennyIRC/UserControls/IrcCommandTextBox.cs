using spennyIRC.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace spennyIRC.UserControls;

public class IrcCommandTextBox : TextBox
{
    private readonly List<string> _history = [];
    private int _currentHistoryIndex = 0;
    private string _currentText = string.Empty;

    public IrcCommandTextBox()
    {
        PreviewKeyDown += IrcCommandTextBox_PreviewKeyDown;
        _currentHistoryIndex = _history.Count;
    }

    private void IrcCommandTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            string command = Text;

            ((IChatWindow)DataContext).ExecuteCommand.Execute(null);

            if (!string.IsNullOrWhiteSpace(command))
            {
                _history.Add(command);
                if (_history.Count > 1000)
                {
                    _history.RemoveAt(0);
                }
            }

            _currentHistoryIndex = _history.Count;
            _currentText = string.Empty;

            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            if (_history.Count == 0) return;

            if (_currentHistoryIndex == _history.Count)
            {
                _currentText = Text;
            }

            if (_currentHistoryIndex > 0)
            {
                _currentHistoryIndex--;
                Text = _history[_currentHistoryIndex];
                CaretIndex = Text.Length;
            }

            e.Handled = true;
            return;
        }

        if (e.Key == Key.Down)
        {
            if (_history.Count == 0) return;

            if (_currentHistoryIndex < _history.Count - 1)
            {
                _currentHistoryIndex++;
                Text = _history[_currentHistoryIndex];
                CaretIndex = Text.Length;
            }
            else if (_currentHistoryIndex == _history.Count - 1)
            {
                _currentHistoryIndex = _history.Count;
                Text = _currentText;
                CaretIndex = Text.Length;
            }

            e.Handled = true;
            return;
        }
    }
}