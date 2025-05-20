using System.Windows;
using System.Windows.Controls;

namespace spennyIRC;

/// <summary>
/// Interaction logic for ServerTreeView.xaml
/// </summary>
public partial class ServerTreeViewControl : UserControl
{
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(ServerTreeViewControl),
            new PropertyMetadata(default(object)));

    public ServerTreeViewControl()
    {
        InitializeComponent();
    }

    public object SelectedItem
    {
        get { return GetValue(SelectedItemProperty); }
        set { SetValue(SelectedItemProperty, value); }
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedItem = e.NewValue;
    }
}
