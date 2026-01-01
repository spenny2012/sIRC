using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting.Commands;
using spennyIRC.ViewModels.Helpers;
using spennyIRC.ViewModels.Messages;
using spennyIRC.ViewModels.Messages.Channel;
using spennyIRC.ViewModels.Messages.LocalUser;
using spennyIRC.ViewModels.Messages.Server;
using spennyIRC.ViewModels.Messages.Window;
using System.Collections.ObjectModel;

namespace spennyIRC.ViewModels;

public class ChannelViewModel : WindowViewModelBase
{
    private static readonly char[] channelStatusChars = ['@', '%', '+', '~', '&'];
    private string _channelTopic = string.Empty;
    private string _channel = string.Empty;
    private ObservableCollection<string> _nickList = [];
    private IEnumerable<string> _selectedNicks = [];
    private IWindowService _echoSvc;

    public ChannelViewModel(IIrcSession session, IIrcCommands commands, string channel) : base(session, commands)
    {
        _echoSvc = session.WindowService;
        Name = Channel = Caption = channel;
    }

    public string Channel
    {
        get => _channel;
        set => SetProperty(ref _channel, value);
    }

    public string ChannelTopic
    {
        get => _channelTopic;
        set => SetProperty(ref _channelTopic, value);
    }

    public IWindowService EchoService
    {
        get => _echoSvc;
        set => SetProperty(ref _echoSvc, value);
    }

    public IEnumerable<string> SelectedNicks //TODO: this is broken
    {
        get { return _selectedNicks; }
        set => SetProperty(ref _selectedNicks, value);
    }

    public ObservableCollection<string> NickList
    {
        get { return _nickList; }
        set => SetProperty(ref _nickList, value);
    }

    protected override void RegisterUISubscriptions()
    {
        WeakReferenceMessenger.Default.Register<ChannelAddMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.Invoke(() => IsSelected = true);
        });

        WeakReferenceMessenger.Default.Register<ServerDisconnectedMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            ThreadSafeInvoker.Invoke(() => NickList.Clear());
        });

        WeakReferenceMessenger.Default.Register<ChannelKickMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            if (m.KickedNick == _session.LocalUser.Nick)
            {
                ThreadSafeInvoker.Invoke(() => NickList.Clear());
                return;
            }
            ThreadSafeInvoker.Invoke(() => NickList.Remove(m.KickedNick));
        });

        WeakReferenceMessenger.Default.Register<ChannelJoinMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.Invoke(() =>
            {
                NickList.Add(m.Nick);
                SortNickList();
            });
        });

        WeakReferenceMessenger.Default.Register<ChannelTopicChangeMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.Invoke(() => ChannelTopic = m.Topic);
        });

        WeakReferenceMessenger.Default.Register<ChannelTopicMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.Invoke(() => ChannelTopic = m.Topic);
        });

        WeakReferenceMessenger.Default.Register<ChannelAddNicksMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.Invoke(() =>
            {
                for (int i = 0; i < m.Nicks.Length; i++)
                {
                    string nick = m.Nicks[i];
                    NickList.Add(nick);
                }
                SortNickList();
            });
        });

        WeakReferenceMessenger.Default.Register<ChannelPartMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            string? foundNick = FindNick(m.Nick);
            if (foundNick == null) return;
            ThreadSafeInvoker.Invoke(() => NickList.Remove(foundNick));
        });

        WeakReferenceMessenger.Default.Register<UserQuitMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            string? foundNick = FindNick(m.Nick);
            if (foundNick == null) return;
            ThreadSafeInvoker.Invoke(() => NickList.Remove(foundNick));
            _session.WindowService.Echo(Channel, $"»» {m.Nick} ({m.Host}) has quit IRC ({m.Message})");
        });

        WeakReferenceMessenger.Default.Register<LocalUserNickChangeMessage>(this, (r, m) =>
        {
            if (m.Session != _session || FindNick(m.Nick) == null) return;
            ThreadSafeInvoker.Invoke(() => ChangeNick(m.Nick, m.NewNick));
        });

        WeakReferenceMessenger.Default.Register<NickChangedMessage>(this, (r, m) =>
        {
            if (m.Session != _session || FindNick(m.Nick) == null) return;
            m.Session.WindowService.Echo(Channel, $"* {m.Nick} is now known as {m.NewNick}");
            ThreadSafeInvoker.Invoke(() =>
            {
                ChangeNick(m.Nick, m.NewNick);
            });
        });
    }

    /// <returns>The full nick with status char if found, otherwise null</returns>
    public string? FindNick(string nick)
    {
        for (int i = 0; i < NickList.Count; i++)
        {
            string currentNick = NickList[i];
            if (channelStatusChars.Contains(currentNick[0]) && currentNick.TrimStart(channelStatusChars).Equals(nick))
                return currentNick;
            else if (currentNick.Equals(nick))
                return currentNick;
        }

        return null;
    }

    //public string? ChangeNickAccess(string nick)
    //{
    //  var
    //}

    private void ChangeNick(string oldNick, string newNick)
    {
        for (int i = 0; i < NickList.Count; i++)
        {
            string current = NickList[i];
            if (current == oldNick)
            {
                UpdateNickAtIndex(i, current, newNick);
                return;
            }

            string currentTrimmed = current.TrimStart(channelStatusChars);
            if (currentTrimmed == oldNick)
            {
                UpdateNickAtIndex(i, current, newNick);
                return;
            }
        }
    }

    private void UpdateNickAtIndex(int index, string oldNick, string newNick)
    {
        NickList[index] = channelStatusChars.Contains(oldNick[0])
            ? oldNick[0] + newNick
            : newNick;

        SortNickList();
    }

    public void SortNickList() => NickList.UserAccessSort();
}