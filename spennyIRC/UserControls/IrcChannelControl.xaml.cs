using spennyIRC.ViewModels;
using System.Windows.Controls;
using System.Windows.Documents;

namespace spennyIRC;

/// <summary>
/// Interaction logic for IrcServerControl.xaml
/// </summary>
public partial class IrcChannelControl : UserControl
{
    private Paragraph paragraph;
    private ChannelViewModel _vm;

    public IrcChannelControl()
    {
        InitializeComponent();
        InitializeChatDisplay();
    }

    private void InitializeChatDisplay()
    {
        paragraph = new Paragraph
        {
            TextAlignment = System.Windows.TextAlignment.Justify
        };
        ChatDisplay.Document.Blocks.Clear();
        ChatDisplay.Document.Blocks.Add(paragraph);
    }

    // TODO: handle all registerecho calls in a different control
    private void RegisterEcho()
    {
        _vm.EchoService.DoEcho += (window, txt) =>
        {
            if (window == _vm.Name || window == "All")
            {
                WriteLine(txt);
            }
        };
    }

    public void WriteLine(string text)
    {
        Dispatcher.Invoke(() =>
        {
            paragraph.Inlines.Add(new Run(text + Environment.NewLine));

            ChatDisplay.ScrollToEnd();
        });
    }

    private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (DataContext == null)
        {
            // Unbind

            return;
        }

        _vm = (ChannelViewModel)DataContext;

        RegisterEcho();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {

    }

    private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.IsDown && e.Key == System.Windows.Input.Key.Enter)
            _vm.ExecuteCommand.Execute(null);
    }
}
