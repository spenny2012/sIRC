namespace spennyIRC.Core.IRC;

//TODO: finish implementing this
public class IrcInternalAddressList : IIrcInternalAddressList
{
    private static readonly IrcUserFactory _userFactory = new();
    private Dictionary<string, IIrcUser> Addresses { get; } = [];

    public void Clear() => Addresses.Clear();

    public void InsertUser(IrcExtractedUserInfo user) // Handles notify online
    {
        if (!Addresses.ContainsKey(user.Nick))
            Addresses[user.Nick] = _userFactory.CreateUser(user);
    }

    public void UpsertUser(IrcExtractedUserInfo user, string channel) // Handles numeric 352
    {
        if (Addresses.TryGetValue(user.Nick, out IIrcUser? userInfo))
        {
            if (user.Ident != null)
                userInfo.Ident = user.Ident;

            if (user.Domain != null)
                userInfo.Domain = user.Domain;

            if (userInfo.Channels.TryGetValue(channel, out IIrcUserChannel? foundChannel))
                foundChannel.Access = [.. user.Access];
            else
                userInfo.Channels[channel] = new IrcUserChannel { Access = [.. user.Access] };
        }
        else
        {
            // TODO: check for host entry before creation
            IIrcUser ircUser = _userFactory.CreateUser(user);
            ircUser.Channels[channel] = new IrcUserChannel { Access = [.. user.Access] };
            Addresses[user.Nick] = ircUser;
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
        if (Addresses.TryGetValue(nick, out IIrcUser? userInfo))
        {
            if (!userInfo.Channels.TryGetValue(channel, out _))
                userInfo.Channels[channel] = new IrcUserChannel();
            else
            {
                IIrcUserChannel foundChannel = userInfo.Channels[channel];
                foundChannel.Access.Clear();
            }
        }
        else
        {
            IIrcUser ircUser = _userFactory.CreateUser(nick, channel);
            Addresses[nick] = ircUser;
        }
    }

    public void RemoveChannel(string nick, string channel) // Handle part
    {
        if (!Addresses.TryGetValue(nick, out IIrcUser? userInfo) || !userInfo.Channels.TryGetValue(channel, out _))
            return;

        userInfo.Channels.Remove(channel);
    }

    public void ChangeNick(string nick, string newNick) // Handle nick change
    {
        if (!Addresses.TryGetValue(nick, out IIrcUser? userInfo)) return;

        Addresses.Remove(nick);
        Addresses[newNick] = userInfo;
    }

    public void RemoveNick(string nick) // Handle quit
    {
        if (!Addresses.TryGetValue(nick, out IIrcUser? userInfo)) return;

        Addresses.Remove(nick);
    }
}
