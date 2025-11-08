using spennyIRC.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace spennyIRC.UserControls;

public class IrcTreeServerTreeView : TreeView
{
    public IrcTreeServerTreeView()
    {
        Loaded += CustomTreeView_Loaded;
    }

    private void CustomTreeView_Loaded(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is ObservableCollection<ServerViewModel> collection)
        {
            collection.CollectionChanged += OnCollectionChanged;
            foreach (ServerViewModel svm in collection)
            {
                svm.Channels.CollectionChanged += OnCollectionChanged;
            }
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (object? newItem in e.NewItems)
            {
                if (newItem is ServerViewModel svm)
                {
                    svm.Channels.CollectionChanged += OnCollectionChanged;
                }
                OnItemAdded(newItem);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (object? oldItem in e.OldItems)
            {
                if (oldItem is ServerViewModel svm)
                {
                    svm.Channels.CollectionChanged -= OnCollectionChanged;
                }
                OnItemRemoved(oldItem);
            }
        }
    }

    protected virtual void OnItemAdded(object newItem)
    {
        IrcContentControlCache.AddControlAndKey((IChatWindow) newItem);
    }

    protected virtual void OnItemRemoved(object oldItem)
    {
        IrcContentControlCache.RemoveControlAndKey((IChatWindow) oldItem);
    }
}