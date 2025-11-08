using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace spennyIRC.UserControls;

public class MultiSelectListbox : ListBox
{
    public MultiSelectListbox()
    {
        SelectionChanged += MultiSelectListbox_SelectionChanged;
    }

    private void MultiSelectListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedItemsList = SelectedItems;
    }

    public IList SelectedItemsList
    {
        get { return (IList) GetValue(SelectedItemsListProperty); }
        set { SetValue(SelectedItemsListProperty, value); }
    }

    public static readonly DependencyProperty SelectedItemsListProperty =
       DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList), typeof(MultiSelectListbox), new PropertyMetadata(null));
}