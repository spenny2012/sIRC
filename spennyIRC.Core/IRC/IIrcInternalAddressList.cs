namespace spennyIRC.Core.IRC;

public interface IIrcInternalAddressList
{
    /// <summary>
    /// Clear the internal address list
    /// </summary>
    void Clear();
    /// <summary>
    /// Handle notify and PRIVMSG
    /// </summary>
    void InsertUser(IrcExtractedUserInfo user);
    /// <summary>
    /// Handle server numeric 352 - WHO reply
    /// </summary>
    void UpsertUser(IrcExtractedUserInfo user, string channel);
    /// <summary>
    /// Handle server numeric 353 - List of users in a channel
    /// </summary>
    void UpsertUsers(IrcExtractedUserInfo[] users, string channel);
    /// <summary>
    /// Handle join
    /// </summary>
    void UpsertChannel(string nick, string channel);
    /// <summary>
    /// Handle part and kick
    /// </summary>
    void RemoveChannel(string nick, string channel);
    /// <summary>
    /// Handle nick change
    /// </summary>
    void ChangeNick(string nick, string newNick);
    /// <summary>
    /// Handle quit and notify offline
    /// </summary>
    void RemoveNick(string nick);
    IIrcUser? FindUserByNick(string nick);
    IIrcUser? FindUserByHost(string host);
}

