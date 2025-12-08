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
    private Paragraph _paragraph;

    public IrcQueryControl()
    {
        InitializeComponent();
        InitializeChatDisplay();
    }

    private void InitializeChatDisplay()
    {
        _paragraph = new Paragraph
        {
            TextAlignment = System.Windows.TextAlignment.Justify
        };
        ChatDisplay.Document.Blocks.Clear();
        ChatDisplay.Document.Blocks.Add(_paragraph);
    }

    // TODO: handle all registerecho calls in a different control

    public void WriteLine(string text)
    {
        Dispatcher.Invoke(() =>
        {
            _paragraph.Inlines.Add(new Run(text + Environment.NewLine));
            ChatDisplay.ScrollToEnd();
        });
    }

    private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue == null)
        {
            if (e.OldValue != null)
            {
                ChannelViewModel oldVm = (ChannelViewModel) e.OldValue;
                oldVm.EchoService.DoEcho -= DoEcho;
                oldVm.EchoService.DoClear -= DoClear;
            }
            return;
        }
        _vm = (QueryViewModel) DataContext;
        _vm.Session.WindowService.DoEcho += DoEcho;
        _vm.Session.WindowService.DoClear += DoClear;
    }

    private void DoEcho(string window, string txt)
    {
        if (window.Equals(_vm.Name, StringComparison.OrdinalIgnoreCase) || window == "All")
        {
            WriteLine(txt);
        }
    }

    private void DoClear(string window)
    {
        if (window == _vm.Name || window == "All")
        {
            ChatDisplay.Document.Blocks.Clear();
            _paragraph.Inlines.Clear();
            _paragraph = null!;
            _paragraph = new Paragraph
            {
                TextAlignment = System.Windows.TextAlignment.Justify
            };
            ChatDisplay.Document.Blocks.Add(_paragraph);
        }
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
    }
}