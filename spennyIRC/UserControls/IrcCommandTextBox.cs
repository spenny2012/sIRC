using spennyIRC.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace spennyIRC.UserControls;

public class IrcCommandTextBox : TextBox
{
    public IrcCommandTextBox() : base()
    {
        KeyDown += IrcCommandTextBox_KeyDown;
        TabIndex = 0;
    }

    private void IrcCommandTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.IsDown && e.Key == Key.Enter)
            ((IChatWindow)DataContext).ExecuteCommand.Execute(null);

        //else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.B))
        //{
        //    Text = Text.Insert(CaretIndex, CharacterConstants.BOLD_CHAR);
        //    CaretIndex++;
        //}
        //else if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.U))
        //{
        //    Text = Text.Insert(CaretIndex, CharacterConstants.UNDERLINE_CHAR);
        //    CaretIndex++;
        //}
        // TODO: add insert color code, bold, underline, and italic

    }
}
