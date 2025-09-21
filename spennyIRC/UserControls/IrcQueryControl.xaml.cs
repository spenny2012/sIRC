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
        _vm.EchoService.DoEcho += DoEcho;
    }

    private void DoEcho(string window, string text)
    {
        if (window == _vm.Name || window == "All")
        {
            WriteLine(text);
        }
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
        if (e.NewValue == null && e.OldValue != null)
        {
            var vm = (QueryViewModel)e.OldValue;
            if (vm.EchoService.DoEcho != null)
                vm.EchoService.DoEcho -= DoEcho;
        }

        _vm = (QueryViewModel)DataContext;

        RegisterEcho();
    }
}