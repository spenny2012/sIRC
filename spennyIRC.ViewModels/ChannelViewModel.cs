using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core.IRC;
using spennyIRC.Scripting;
using spennyIRC.ViewModels.Helpers;
using spennyIRC.ViewModels.Messages;
using spennyIRC.ViewModels.Messages.Channel;
using spennyIRC.ViewModels.Messages.LocalUser;
using spennyIRC.ViewModels.Messages.Server;
using System.Collections.ObjectModel;
using System.Threading.Channels;

namespace spennyIRC.ViewModels;

public class ChannelViewModel : WindowViewModelBase
{
    private static readonly char[] channelStatusChars = ['~', '&', '@', '%', '+'];
    private static readonly HashSet<char> statusCharsSet = new(channelStatusChars);
    private string _channelTopic = string.Empty;
    private string _channel = string.Empty;
    private ObservableCollection<string> _nickList = [];
    private IEnumerable<string> _selectedNicks = [];
    private IEchoService _echoSvc;

    public ChannelViewModel(IIrcSession session, IIrcCommands commands, string channel) : base(session, commands)
    {
        _echoSvc = session.EchoService;
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

    public IEchoService EchoService
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
        WeakReferenceMessenger.Default.Register<ServerDisconnectedMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            ThreadSafeInvoker.InvokeIfNecessary(() => NickList.Clear());
        });

        WeakReferenceMessenger.Default.Register<ChannelKickMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            if (m.KickedNick == _session.LocalUser.Nick)
            {
                ThreadSafeInvoker.InvokeIfNecessary(() => NickList.Clear());
            }
        });

        WeakReferenceMessenger.Default.Register<ChannelJoinMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.InvokeIfNecessary(() =>
            {
                NickList.Add(m.Nick);
                SortNickList();
            });
        });

        WeakReferenceMessenger.Default.Register<ChannelTopicChangeMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.InvokeIfNecessary(() => ChannelTopic = m.Topic);
        });

        WeakReferenceMessenger.Default.Register<ChannelTopicMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.InvokeIfNecessary(() => ChannelTopic = m.Topic);
        });

        WeakReferenceMessenger.Default.Register<ChannelAddNicksMessage>(this, (r, m) =>
        {
            if (m.Session != _session || m.Channel != Channel) return;
            ThreadSafeInvoker.InvokeIfNecessary(() =>
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
            ThreadSafeInvoker.InvokeIfNecessary(() => NickList.Remove(foundNick));
        });

        WeakReferenceMessenger.Default.Register<UserQuitMessage>(this, (r, m) =>
        {
            if (m.Session != _session) return;
            string? foundNick = FindNick(m.Nick);
            if (foundNick == null) return;
            ThreadSafeInvoker.InvokeIfNecessary(() => NickList.Remove(foundNick));
            _session.EchoService.Echo(Channel, $"»» {m.Nick} ({m.Host}) has quit IRC ({m.Message})");
        });

        WeakReferenceMessenger.Default.Register<LocalUserNickChangeMessage>(this, (r, m) =>
        {
            if (m.Session != _session || FindNick(m.Nick) == null) return;
            ThreadSafeInvoker.InvokeIfNecessary(() => ChangeNick(m.Nick, m.NewNick));
        });
    }

    /// <summary>
    /// Find a nick in the nick list and return the full nick with status char if found
    /// </summary>
    /// <param name="nick">Nick without status char.</param>
    /// <returns>The full nick with status char if found</returns>
    public string? FindNick(string nick)
    {
        for (int i = 0; i < NickList.Count; i++)
        {
            string currentNick = NickList[i];
            if (currentNick.TrimStart(channelStatusChars).Equals(nick))
            {
                return NickList[i];
            }
        }

        return null;
    }

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
        string updatedNick = statusCharsSet.Contains(oldNick[0])
            ? oldNick[0] + newNick
            : newNick;

        NickList[index] = updatedNick;

        SortNickList();
    }

    public void SortNickList() => NickList.UserAccessSort();
}