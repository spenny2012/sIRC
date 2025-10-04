using System.Windows;
using System.Windows.Controls;

namespace spennyIRC;

/// <summary>
/// Interaction logic for MainIrcWindowControl.xaml
/// </summary>
public partial class MainIrcWindowControl : UserControl
{
    public MainIrcWindowControl()
    {
        InitializeComponent();
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
    }
}