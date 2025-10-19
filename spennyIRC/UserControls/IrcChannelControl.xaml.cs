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
        if (e.NewValue == null)
        {
            if (e.OldValue != null)
            {
                ChannelViewModel oldVm = (ChannelViewModel)e.OldValue;
                oldVm.EchoService.DoEcho -= DoEcho;
                oldVm.EchoService.DoClear -= DoClear;
            }
            return;
        }
        _vm = (ChannelViewModel)DataContext;
        _vm.EchoService.DoEcho += DoEcho;
        _vm.EchoService.DoClear += DoClear;
    }

    private void DoEcho(string window, string txt)
    {
        if (window == _vm.Name || window == "All")
        {
            WriteLine(txt);
        }
    }

    private void DoClear(string window)
    {
        if (window == _vm.Name || window == "All")
        {
            ChatDisplay.Document.Blocks.Clear();
        }
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
    }
}