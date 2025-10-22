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
        _vm.WindowService.DoEcho += DoEcho;
    }

    private void DoEcho(string window, string text)
    {
        if (window.Equals(_vm.Name, StringComparison.OrdinalIgnoreCase) || window == "All")
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
            QueryViewModel vm = (QueryViewModel)e.OldValue;
            if (vm.WindowService.DoEcho != null)
                vm.WindowService.DoEcho -= DoEcho;
        }

        _vm = (QueryViewModel)DataContext;

        RegisterEcho();
    }
}