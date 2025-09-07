using spennyIRC.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace spennyIRC.UserControls;

public class IrcCommandTextBox : TextBox
{
    private readonly List<string> _history = new();
    private int _currentHistoryIndex = 0;
    private string _currentText = "";

    public IrcCommandTextBox()
    {
        KeyDown += IrcCommandTextBox_KeyDown;
        _currentHistoryIndex = _history.Count;
    }

    private void IrcCommandTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Store the current text before clearing
            string command = Text;

            // Execute the command
            ((IChatWindow)DataContext).ExecuteCommand.Execute(null);

            // Clear the textbox
            Text = "";

            // Add to history if not empty
            if (!string.IsNullOrWhiteSpace(command))
            {
                _history.Add(command);
                if (_history.Count > 1000)
                {
                    _history.RemoveAt(0);
                }
            }

            _currentHistoryIndex = _history.Count;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            if (_history.Count == 0) return;

            // Save current text if we're at the end of history
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
            else
            {
                _currentHistoryIndex = _history.Count;
                Text = _currentText;
            }

            e.Handled = true;
            return;
        }
    }
}