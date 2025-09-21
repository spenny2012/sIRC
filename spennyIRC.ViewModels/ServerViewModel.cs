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

        RegisterUISubscriptions();
    }

    ~ServerViewModel()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    public ObservableCollection<IChatWindow> Channels // TODO: rename to Windows
    {
        get => _channels;
        set => SetProperty(ref _channels, value);
    }

    public void CloseWindow(string name)
    {
        IChatWindow? window = FindWindowByName(_channels, name);
        if (window == null) return;

        Channels.Remove(window);

        window.Dispose();
    }

    #region UI Subscriptions

    private void RegisterUISubscriptions() // TODO: Reconsider threadsafe invoker
    {
        WeakReferenceMessenger.Default.Register<ChannelJoinMessage>(this, (r, m) =>
        {
            if (m.Session != _session || !FindWindowByName(Channels, m.Channel, out ChannelViewModel channel)) return;
            ThreadSafeInvoker.InvokeIfNecessary(() =>
            {
                channel.NickList.Add(m.Nick);
                channel.SortNickList();
            });
        });

        WeakReferenceMessenger.Default.Register<ChannelPartMessage>(this, (r, m) =>
        {
            if (m.Session != _session || !FindWindowByName(Channels, m.Channel, out ChannelViewModel channel)) return;
            if (m.Nick == _user.Nick)
            {
                ThreadSafeInvoker.InvokeIfNecessary(() =>
                {
                    Channels.Remove(channel);
                });
                return;
            }

            string? foundNick = channel.FindNick(m.Nick);

            if (foundNick == null) return;

            ThreadSafeInvoker.InvokeIfNecessary(() =>
            {
                channel.NickList.Remove(foundNick);
            });
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

        WeakReferenceMessenger.Default.Register<ChannelAddNicksMessage>(this, (r, m) =>
        {
            if (m.Session != _session || !FindWindowByName(Channels, m.Channel, out ChannelViewModel channel)) return;
            ThreadSafeInvoker.InvokeIfNecessary(() =>
            {
                for (int i = 0; i < m.Nicks.Length; i++)
                {
                    string nick = m.Nicks[i];
                    channel.NickList.Add(nick);
                }

                channel.SortNickList();
            });
        });

        WeakReferenceMessenger.Default.Register<ChannelTopicMessage>(this, (r, m) =>
        {
            if (m.Session != _session || !FindWindowByName(Channels, m.Channel, out ChannelViewModel channel)) return;
            ThreadSafeInvoker.InvokeIfNecessary(() =>
            {
                channel.ChannelTopic = m.Topic;
            });
        });

        WeakReferenceMessenger.Default.Register<ServerDisconnectedMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            ThreadSafeInvoker.InvokeIfNecessary(() =>
            {
                for (int i = 0; i < Channels.Count; i++)
                {
                    IChatWindow? channel = Channels[i];
                    if (channel is ChannelViewModel cvm)
                        cvm.NickList.Clear();
                }
                Caption = "(No Network)";
            });
        });

        WeakReferenceMessenger.Default.Register<UserQuitMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;

            for (int i = 0; i < Channels.Count; i++)
            {
                IChatWindow channel = Channels[i]; // TODO: find a better way to do this
                if (channel is ChannelViewModel cvm)
                {
                    string? foundNick = cvm.FindNick(m.Nick);

                    if (foundNick == null) continue;

                    ThreadSafeInvoker.InvokeIfNecessary(() =>
                    {
                        cvm.NickList.Remove(foundNick);
                    });

                    cvm.EchoService.Echo(cvm.Channel, $"»» {m.Nick} ({m.Host}) has quit IRC ({m.Message})");
                }
                else if (channel is QueryViewModel qvm && m.Nick == qvm.Name)
                {
                    qvm.EchoService.Echo(qvm.Name, $"»» {m.Nick} ({m.Host}) has quit IRC ({m.Message})");
                }
            }
        });

        WeakReferenceMessenger.Default.Register<ChannelTopicChangeMessage>(this, (r, m) =>
        {
            if (m.Session != _session || !FindWindowByName(Channels, m.Channel, out ChannelViewModel channel)) return;
            ThreadSafeInvoker.InvokeIfNecessary(() =>
            {
                channel.ChannelTopic = m.Topic;
            });
        });

        WeakReferenceMessenger.Default.Register<QueryMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            if (!FindWindowByName(Channels, m.Nick, out QueryViewModel nickWindow))
            {
                ThreadSafeInvoker.InvokeIfNecessary(() =>
                {
                    Channels.Add(new QueryViewModel(_session, _commands, m.Nick));
                });
            }
        });

        WeakReferenceMessenger.Default.Register<ServerISupportMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            if (!string.IsNullOrEmpty(_server.Network))
                Caption = _server.Network;
        });

        WeakReferenceMessenger.Default.Register<ChannelKickMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            if (m.KickedNick == _user.Nick)
            {
                if (FindWindowByName(Channels, m.Channel, out ChannelViewModel channel))
                {
                    ThreadSafeInvoker.InvokeIfNecessary(() =>
                    {
                        channel.NickList.Clear();
                    });
                }
            }
        });
        WeakReferenceMessenger.Default.Register<LocalUserNickChangeMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;

            for (int i = 0; i < Channels.Count; i++)
            {
                IChatWindow channel = Channels[i];
                if (channel is ChannelViewModel cvm)
                {
                    ThreadSafeInvoker.InvokeIfNecessary(() =>
                    {
                        cvm.ChangeNick(m.Nick, m.NewNick);
                    });
                }
                else if (channel is QueryViewModel qvm && m.Nick == qvm.Name)
                {
                    ThreadSafeInvoker.InvokeIfNecessary(() =>
                    {
                        qvm.Name = qvm.Caption = m.NewNick;
                    });
                }
            }
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

    private static IChatWindow? FindWindowByName(ObservableCollection<IChatWindow> windows, string name)
    {
        return windows.FirstOrDefault(c => c.Name == name);
    }

    #endregion Helper
}