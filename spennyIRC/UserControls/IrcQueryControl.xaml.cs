using spennyIRC.ViewModels;
using System.Windows.Controls;
using System.Windows.Documents;

namespace spennyIRC.UserControls;

/// <summary>
/// Interaction logic for IrcQueryControl.xaml
/// </summary>
public partial class IrcQueryControl : UserControl
{
    private QueryViewModel _vm;
    private Paragraph paragraph;
    public IrcQueryControl()
    {
        InitializeComponent();
        InitializeChatDisplay();
    }

    private void InitializeChatDisplay()
    {
        paragraph = new Paragraph();
        ChatDisplay.Document.Blocks.Clear();
        ChatDisplay.Document.Blocks.Add(paragraph);
    }

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

        _vm = (QueryViewModel)DataContext;

        RegisterEcho();
    }
}
