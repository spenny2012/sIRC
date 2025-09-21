using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting;
using spennyIRC.ViewModels.Helpers;
using spennyIRC.ViewModels.Messages.Channel;
using spennyIRC.ViewModels.Messages.LocalUser;
using spennyIRC.ViewModels.Messages.Server;
using System.Collections.ObjectModel;

// TODO: properly handle the disposing of ServerViewModel and other classes
namespace spennyIRC.ViewModels;

public class ServerViewModel : WindowViewModelBase
{
    private readonly IIrcServer _server;
    private readonly IIrcLocalUser _localUser;
    private ObservableCollection<IChatWindow> _channels = [];

    public ServerViewModel(IIrcSession session, IIrcCommands commands) : base(session, commands)
    {
        _server = session.Server;
        _localUser = session.LocalUser;
        Name = "Status";
        Caption = "(No Network)";
    }

    public ObservableCollection<IChatWindow> Channels // TODO: rename to Windows
    {
        get => _channels;
        set => SetProperty(ref _channels, value);
    }

    #region UI Subscriptions

    protected override void RegisterUISubscriptions()
    {
        WeakReferenceMessenger.Default.Register<ChannelPartMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Nick != _localUser.Nick || !FindWindowByName(Channels, m.Channel, out ChannelViewModel channel))
                return;

            ThreadSafeInvoker.InvokeIfNecessary(() => Channels.Remove(channel));
        });

        WeakReferenceMessenger.Default.Register<ChannelAddMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            if (FindWindowByName(Channels, m.Channel, out ChannelViewModel channel))
            {
                channel.IsSelected = true;
                return;
            }

            ThreadSafeInvoker.InvokeIfNecessary(() =>
            {
                ChannelViewModel channel = new(_session, _commands, m.Channel);
                Channels.Add(channel);
                Channels.AlphaNumericSort();
                channel.IsSelected = true;
            });
        });

        WeakReferenceMessenger.Default.Register<ServerDisconnectedMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            ThreadSafeInvoker.InvokeIfNecessary(() => Caption = "(No Network)");
        });

        WeakReferenceMessenger.Default.Register<QueryMessage>(this, (r, m) =>
        {
            if (m.Session != _session || FindWindowByName(Channels, m.Nick, out QueryViewModel _)) return;
            ThreadSafeInvoker.InvokeIfNecessary(() => Channels.Add(new QueryViewModel(_session, _commands, m.Nick)));
        });

        WeakReferenceMessenger.Default.Register<ServerISupportMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            //if (!string.IsNullOrEmpty(_server.Network))
            //if (!string.IsNullOrEmpty(_server.Network))
            Caption = _server.Network;
        });
    }

    #endregion UI Subscriptions

    #region Helper

    private static bool FindWindowByName<T>(ObservableCollection<IChatWindow> windows, string name, out T value)
        where T : IChatWindow
    {
        value = default!;
        if (windows.FirstOrDefault(c => c.Name == name) is T channel)
        {
            value = channel;
            return true;
        }
        return false;
    }

    #endregion Helper
}