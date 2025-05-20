namespace spennyIRC.Core.IRC;

public class IrcUserFactory
{
    public IIrcUser CreateUser(string nick, string channel)
    {
        IrcUser ircUser = new()
        {
            Nick = nick
        };
        ircUser.Channels[channel] = new IrcUserChannel();
        return ircUser;
    }

    public IIrcUser CreateUser(string nick, string ident, string domain)
    {
        IrcUser ircUser = new()
        {
            Nick = nick,
            Ident = ident,
            Domain = domain
        };
        return ircUser;
    }

    public IIrcUser CreateUser(IrcExtractedUserInfo userInfo)
    {
        IrcUser ircUser = new()
        {
            Nick = userInfo.Nick,
            Ident = userInfo.Ident,
            Domain = userInfo.Domain
        };
        return ircUser;
    }
}
