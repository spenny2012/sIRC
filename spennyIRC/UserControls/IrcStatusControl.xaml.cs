using spennyIRC.ViewModels;
using System.Windows.Controls;
using System.Windows.Documents;

namespace spennyIRC;

/// <summary>
/// Interaction logic for IrcStatusControl.xaml
/// </summary>
public partial class IrcStatusControl : UserControl
{
    private Paragraph paragraph;
    private ServerViewModel _vm;

    public IrcStatusControl()
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
        _vm.Session.WindowService.DoEcho += (window, txt) =>
        {
            if (window.Equals(_vm.Name, StringComparison.OrdinalIgnoreCase) || window == "All")
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

        _vm = (ServerViewModel) DataContext;

        RegisterEcho();
    }

    private void UserControl_LayoutUpdated(object sender, EventArgs e)
    {
    }
}