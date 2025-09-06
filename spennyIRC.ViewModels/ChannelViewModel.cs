using spennyIRC.Core.IRC;
using spennyIRC.Scripting;
using spennyIRC.ViewModels.Helpers;
using System.Collections.ObjectModel;

namespace spennyIRC.ViewModels;

public class ChannelViewModel : WindowViewModelBase
{
    private static readonly char[] channelStatusChars = ['~', '&', '@', '%', '+'];
    private string _channelTopic = string.Empty;
    private string _channel = string.Empty;
    private ObservableCollection<string> _nickList = [];
    private IEnumerable<string> _selectedNicks = [];
    private IEchoService _echoSvc;

    public ChannelViewModel(IIrcSession session, IIrcCommands commands, string channel) : base(session, commands)
    {
        _echoSvc = session.EchoService;
        Name = channel;
        Channel = Caption = channel;
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

    /// <summary>
    /// Change a nick in the nick list, ignoring all status chars
    /// </summary>
    public void ChangeNick(string oldNick, string newNick)
    {
        int index = NickList.IndexOf(oldNick);
        if (index == -1)
        {
            for (int i = 0; i < NickList.Count; i++)
            {
                string currentNick = NickList[i].TrimStart(channelStatusChars);
                if (oldNick.TrimStart(channelStatusChars).Equals(currentNick))
                {
                    index = i;
                    break;
                }
            }
        }

        if (index != -1)
        {
            string statusChar = channelStatusChars.Contains(NickList[index][0])
                ? NickList[index][0].ToString()
                : string.Empty;
            NickList[index] = statusChar + newNick.TrimStart(channelStatusChars);
            SortNickList();
        }
    }

    public void SortNickList()
    {
        NickList.UserAccessSort();
    }
}
//public string? FindNick(string nick)
//{
//    for (int i = 0; i < NickList.Count; i++)
//    {
//        string currentNick = NickList[i];
//        if (channelStatusChars.Contains(currentNick[0]))
//        {
//            currentNick = currentNick[1..];
//        }

//        if (nick.Equals(currentNick))
//        {
//            return NickList[i];
//        }
//    }

//    return null;
//}