namespace spennyIRC.Core.IRC;

public interface IIrcInternalAddressList
{
    /// <summary>
    /// Handle nick change
    /// </summary>
    void ChangeNick(string nick, string newNick);
    /// <summary>
    /// Clear the internal address list
    /// </summary>
    void Clear();
    /// <summary>
    /// Finds a user by hostname (wildcards don't work)
    /// </summary>
    IIrcUser? FindUserByHost(string host);
    /// <summary>
    /// Finds a user by nick (wildcards don't work)
    /// </summary>
    IIrcUser? FindUserByNick(string nick);
    /// <summary>
    /// Handle notify and PRIVMSG
    /// </summary>
    void InsertUser(IrcExtractedUserInfo user);
    /// <summary>
    /// Handle part and kick
    /// </summary>
    void RemoveChannel(string nick, string channel);
    /// <summary>
    /// Handle quit and notify offline
    /// </summary>
    void RemoveNick(string nick);
    /// <summary>
    /// Handle join
    /// </summary>
    void UpsertChannel(string nick, string channel);
    /// <summary>
    /// Handle server numeric 352 - WHO reply
    /// </summary>
    void UpsertUser(IrcExtractedUserInfo user, string channel);
    /// <summary>
    /// Handle server numeric 353 - List of users in a channel
    /// </summary>
    void UpsertUsers(IrcExtractedUserInfo[] users, string channel);
}

