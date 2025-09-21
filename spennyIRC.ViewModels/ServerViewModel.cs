using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting;
using spennyIRC.ViewModels.Helpers;
using spennyIRC.ViewModels.Messages;
using spennyIRC.ViewModels.Messages.Channel;
using spennyIRC.ViewModels.Messages.LocalUser;
using spennyIRC.ViewModels.Messages.Server;
using System.Collections.ObjectModel;

// TODO: properly handle the disposing of ServerViewModel and other classes
namespace spennyIRC.ViewModels;

public class ServerViewModel : WindowViewModelBase
{
    private readonly IIrcServer _server;
    private readonly IIrcLocalUser _user;
    private ObservableCollection<IChatWindow> _channels = [];

    public ServerViewModel(IIrcSession session, IIrcCommands commands) : base(session, commands)
    {
        _server = session.Server;
        _user = session.LocalUser;
        Name = "Status";
        Caption = "(No Network)";
    }

    public ObservableCollection<IChatWindow> Channels // TODO: rename to Windows
    {
        get => _channels;
        set => SetProperty(ref _channels, value);
    }

    #region UI Subscriptions

    protected override void RegisterUISubscriptions() // TODO: Reconsider threadsafe invoker
    {
        WeakReferenceMessenger.Default.Register<ChannelPartMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            if (m.Nick == _user.Nick)
            {
                if (!FindWindowByName(Channels, m.Channel, out ChannelViewModel channel)) return;
                ThreadSafeInvoker.InvokeIfNecessary(() =>
                {
                    Channels.Remove(channel);
                    channel.Dispose();
                });
            }
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
            if (m.Session != _session) return;
            if (!FindWindowByName(Channels, m.Nick, out QueryViewModel nickWindow))
                ThreadSafeInvoker.InvokeIfNecessary(() => Channels.Add(new QueryViewModel(_session, _commands, m.Nick)));
        });

        WeakReferenceMessenger.Default.Register<ServerISupportMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            if (!string.IsNullOrEmpty(_server.Network))
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