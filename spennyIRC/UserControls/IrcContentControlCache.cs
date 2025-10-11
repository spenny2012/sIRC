using spennyIRC.UserControls;
using spennyIRC.ViewModels;
using System.Windows.Controls;

namespace spennyIRC;

public static class IrcContentControlCache
{
    public static readonly Dictionary<IChatWindow, UserControl> Cache = [];

    public static UserControl AddControlAndKey(IChatWindow chatWindow)
    {
        UserControl ctrl = CreateControl(chatWindow);
        Cache[chatWindow] = ctrl;
        return ctrl;
    }

    public static void RemoveControlAndKey(IChatWindow window)
    {
        if (Cache.TryGetValue(window, out UserControl? userControl))
        {
            userControl.DataContext = null;
            window.Dispose();
            Cache.Remove(window);
        }
    }

    private static UserControl CreateControl(IChatWindow CacheKey)
    {
        UserControl content = CacheKey.GetType().Name switch
        {
            "ChannelViewModel" => new IrcChannelControl(),
            "QueryViewModel" => new IrcQueryControl(),
            "ServerViewModel" => new IrcStatusControl(),
            _ => throw new Exception("Invalid object type"),
        };

        content.DataContext = CacheKey;
        return content;
    }
}