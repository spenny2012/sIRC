using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Core.IRC;

public class IrcInternalAddressList : IIrcInternalAddressList
{
    private static readonly IrcUserFactory _userFactory = new();
    private Dictionary<string, IIrcUser> Nicks { get; } = [];
    private Dictionary<string, IIrcUser> Hosts { get; } = [];

    public void Clear()
    {
        Nicks.Clear();
        Hosts.Clear();
    }

    public void InsertUser(IrcExtractedUserInfo extractedUser) // Handles notify online
    {
        IIrcUser user = _userFactory.CreateUser(extractedUser);

        if (!Nicks.ContainsKey(extractedUser.Nick))
            Nicks[extractedUser.Nick] = user;

        AddHostEntryIfNoneExists(user);
    }

    public void UpsertUser(IrcExtractedUserInfo user, string channel) // Handles numeric 352
    {
        // if nick is found, modify the found entry
        if (Nicks.TryGetValue(user.Nick, out IIrcUser? foundUser))
        {
            if (user.Ident != null)
                foundUser.Ident = user.Ident;

            if (user.Domain != null)
                foundUser.Domain = user.Domain;

            if (foundUser.Channels.TryGetValue(channel, out IIrcUserChannel? foundChannel))
                foundChannel.Access = [.. user.Access];
            else
                foundUser.Channels[channel] = new IrcUserChannel { Access = [.. user.Access] };

            AddHostEntryIfNoneExists(foundUser);
        }
        // no nick was found, create user
        else
        {
            // TODO: check for host entry before creation
            IIrcUser ircUser = _userFactory.CreateUser(user);
            ircUser.Channels[channel] = new IrcUserChannel { Access = [.. user.Access] };
            Nicks[user.Nick] = ircUser;
            AddHostEntryIfNoneExists(ircUser);
        }
    }

    public void UpsertUsers(IrcExtractedUserInfo[] users, string channel) // Handles numeric 353
    {
        foreach (IrcExtractedUserInfo user in users)
        {
            UpsertUser(user, channel);
        }
    }

    public void UpsertChannel(string nick, string channel) // Handle join
    {
        if (Nicks.TryGetValue(nick, out IIrcUser? userInfo))
        {
            if (!userInfo.Channels.TryGetValue(channel, out _))
                userInfo.Channels[channel] = new IrcUserChannel();
            else
            {
                IIrcUserChannel foundChannel = userInfo.Channels[channel];
                foundChannel.Access.Clear();
            }

            return;
        }

        IIrcUser ircUser = _userFactory.CreateUser(nick, channel);
        Nicks[nick] = ircUser;
    }

    public void RemoveChannel(string nick, string channel) // Handle part
    {
        if (!Nicks.TryGetValue(nick, out IIrcUser? userInfo) || !userInfo.Channels.TryGetValue(channel, out _))
            return;

        userInfo.Channels.Remove(channel);
    }

    public void ChangeNick(string nick, string newNick) // Handle nick change
    {
        if (!Nicks.TryGetValue(nick, out IIrcUser? userInfo)) return;

        Nicks.Remove(nick);
        userInfo.Nick = nick;
        Nicks[newNick] = userInfo;
    }

    public void RemoveNick(string nick) // Handle quit
    {
        if (!Nicks.TryGetValue(nick, out IIrcUser? userInfo)) return;

        Nicks.Remove(nick);
        RemoveHostEntryIfExists(userInfo);
    }

    public IIrcUser? FindUserByNick(string nick) => FindBy(Nicks, nick);

    public IIrcUser? FindUserByHost(string host) => FindBy(Hosts, host);

    public IIrcUser[]? QueryIal(string query)
    {
        // extract query bits

        // if there's a nick present, bypass all other checks and user Addresses
        // if no nick but it's a full host without no wildcards, check Hosts
        // if contains a wildcard, loop through Addresses
        throw new NotImplementedException();
    }

    private void AddHostEntryIfNoneExists(IIrcUser user)
    {
        string? host = user.GetHost();
        if (host == null || Hosts.ContainsKey(host)) return;
        Hosts[host] = user;
    }

    private void RemoveHostEntryIfExists(IIrcUser user)
    {
        string? host = user.GetHost();
        if (host == null || !Hosts.ContainsKey(host)) return;
        Hosts.Remove(host);
    }

    private static IIrcUser? FindBy(Dictionary<string, IIrcUser> collection, string index)
    {
        if (!collection.TryGetValue(index, out IIrcUser? found)) return null;

        return found;
    }
}
