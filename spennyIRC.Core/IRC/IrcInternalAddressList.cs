using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Core.IRC;

public class IrcInternalAddressList : IIrcInternalAddressList
{
    private static readonly IrcUserFactory _userFactory = new();
    private Dictionary<string, IIrcUser> Hosts { get; } = [];
    private Dictionary<string, IIrcUser> Nicks { get; } = [];

    public void ChangeNick(string nick, string newNick) // Handle nick change
    {
        if (!Nicks.TryGetValue(nick, out IIrcUser? foundUser)) return;

        Nicks.Remove(nick);
        foundUser.Nick = nick;
        Nicks[newNick] = foundUser;
    }

    public void Clear()
    {
        Nicks.Clear();
        Hosts.Clear();
    }

    public IIrcUser? FindUserByHost(string host) => FindEntry(Hosts, host);

    public IIrcUser? FindUserByNick(string nick) => FindEntry(Nicks, nick);

    public void InsertUser(IrcExtractedUserInfo extractedUser) // Handles notify online
    {
        IIrcUser newUser = _userFactory.CreateUser(extractedUser);

        if (!Nicks.ContainsKey(extractedUser.Nick))
            Nicks[extractedUser.Nick] = newUser;

        AddHostEntryIfNoneExists(newUser);
    }

    public IIrcUser[]? QueryIal(string query)
    {
        // extract query bits

        // if there's a nick present, bypass all other checks and user Addresses
        // if no nick but it's a full host without no wildcards, check Hosts
        // if contains a wildcard, loop through Addresses
        throw new NotImplementedException();
    }

    public void RemoveChannel(string nick, string channel) // Handle part
    {
        if (!Nicks.TryGetValue(nick, out IIrcUser? foundUser) || !foundUser.Channels.TryGetValue(channel, out _))
            return;

        foundUser.Channels.Remove(channel);
    }

    public void RemoveNick(string nick) // Handle quit
    {
        if (!Nicks.TryGetValue(nick, out IIrcUser? foundUser)) return;

        Nicks.Remove(nick);
        RemoveHostEntryIfExists(foundUser);
    }

    public void UpsertChannel(string nick, string channel) // Handle join
    {
        if (Nicks.TryGetValue(nick, out IIrcUser? foundUser))
        {
            if (!foundUser.Channels.TryGetValue(channel, out _))
                foundUser.Channels[channel] = new IrcUserChannel();
            else
            {
                IIrcUserChannel foundChannel = foundUser.Channels[channel];
                foundChannel.Access.Clear();
            }

            return;
        }

        IIrcUser ircUser = _userFactory.CreateUser(nick, channel);
        Nicks[nick] = ircUser;
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
            IIrcUser newUser = _userFactory.CreateUser(user);
            newUser.Channels[channel] = new IrcUserChannel { Access = [.. user.Access] };
            Nicks[user.Nick] = newUser;
            AddHostEntryIfNoneExists(newUser);
        }
    }

    public void UpsertUsers(IrcExtractedUserInfo[] users, string channel) // Handles numeric 353
    {
        foreach (IrcExtractedUserInfo user in users)
        {
            UpsertUser(user, channel);
        }
    }

    #region private

    private static IIrcUser? FindEntry(Dictionary<string, IIrcUser> collection, string index)
    {
        return !collection.TryGetValue(index, out IIrcUser? found) ? null : found;
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

    #endregion private
}