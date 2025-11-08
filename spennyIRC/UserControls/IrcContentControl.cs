using spennyIRC.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace spennyIRC.UserControls;

public class IrcContentControl : ContentControl
{
    public static readonly DependencyProperty CacheKeyProperty =
        DependencyProperty.Register(
            "CacheKey",
            typeof(object),
            typeof(IrcContentControl),
            new PropertyMetadata(null, OnCacheKeyChanged));

    public IrcContentControl()
    {
        Loaded += CachingContentPresenter_Loaded;
    }

    public object CacheKey
    {
        get { return GetValue(CacheKeyProperty); }
        set { SetValue(CacheKeyProperty, value); }
    }

    private static void OnCacheKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        IrcContentControl? presenter = d as IrcContentControl;
        presenter?.UpdateContent();
    }

    private void CachingContentPresenter_Loaded(object sender, RoutedEventArgs e) => UpdateContent();

    private void UpdateContent()
    {
        if (CacheKey == null)
        {
            Content = null;
            return;
        }

        IChatWindow key = (IChatWindow) CacheKey;

        if (IrcContentControlCache.Cache.TryGetValue(key, out UserControl? cachedContent))
            Content = cachedContent;
        else
            Content = IrcContentControlCache.AddControlAndKey(key);
    }
}