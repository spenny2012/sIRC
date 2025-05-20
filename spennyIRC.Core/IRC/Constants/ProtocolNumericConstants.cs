namespace spennyIRC.Core.IRC.Constants;

public static class ProtocolNumericConstants
{
    public const string PROCESSING_REQUEST = "020"; // Processing request message
    // 001 - 099: Basic Replies (Welcome and Server Info)
    public const string RPL_WELCOME = "001";        // Welcome message
    public const string RPL_YOURHOST = "002";       // Your host
    public const string RPL_CREATED = "003";        // Server created message
    public const string RPL_MYINFO = "004";         // Server info
    public const string RPL_ISUPPORT = "005";       // ISUPPORT information (modern usage)

    // 200 - 299: Trace and Stats Replies
    public const string RPL_TRACELINK = "200";      // Trace link
    public const string RPL_TRACECONNECTING = "201"; // Trace connecting
    public const string RPL_TRACEHANDSHAKE = "202"; // Trace handshake
    public const string RPL_TRACEUNKNOWN = "203";   // Trace unknown
    public const string RPL_TRACEOPERATOR = "204";  // Trace operator
    public const string RPL_TRACEUSER = "205";      // Trace user
    public const string RPL_TRACESERVER = "206";    // Trace server
    public const string RPL_TRACESERVICE = "207";   // Trace service
    public const string RPL_TRACENEWTYPE = "208";   // Trace new type
    public const string RPL_TRACECLASS = "209";     // Trace class
    public const string RPL_STATSLINKINFO = "211";  // Link info
    public const string RPL_STATSCOMMANDS = "212";  // Command stats
    public const string RPL_STATSCLINE = "213";     // C-line (client connection)
    public const string RPL_STATSNLINE = "214";     // N-line (server connection)
    public const string RPL_STATSILINE = "215";     // I-line (IP connection)
    public const string RPL_STATSKLINE = "216";     // K-line (banned users)
    public const string RPL_STATSYLINE = "218";     // Y-line (class stats)
    public const string RPL_ENDOFSTATS = "219";     // End of stats
    public const string RPL_UMODEIS = "221";        // User mode
    public const string RPL_STATSCONN = "250";      // Connection stats
    public const string RPL_LUSERCLIENT = "251";    // Client count
    public const string RPL_LUSEROP = "252";        // Operator count
    public const string RPL_LUSERUNKNOWN = "253";   // Unknown connection count
    public const string RPL_LUSERCHANNELS = "254";  // Channel count
    public const string RPL_LUSERME = "255";        // Server user count
    public const string RPL_ADMINME = "256";        // Admin info start
    public const string RPL_ADMINLOC1 = "257";      // Admin location line 1
    public const string RPL_ADMINLOC2 = "258";      // Admin location line 2
    public const string RPL_ADMINEMAIL = "259";     // Admin email
    public const string RPL_TRYAGAIN = "263";       // Try again later
    public const string RPL_LOCALUSERS = "265";     // Local user count
    public const string RPL_GLOBALUSERS = "266";    // Global user count

    // 300 - 399: User and Channel Replies
    public const string RPL_AWAY = "301";           // Away message
    public const string RPL_USERHOST = "302";       // Userhost response
    public const string RPL_ISON = "303";           // Is user online
    public const string RPL_UNAWAY = "305";         // User is no longer away
    public const string RPL_NOWAWAY = "306";        // User is now away
    public const string RPL_WHOISUSER = "311";      // WHOIS user information
    public const string RPL_WHOISSERVER = "312";    // WHOIS server information
    public const string RPL_WHOISOPERATOR = "313";  // WHOIS operator information
    public const string RPL_WHOWASUSER = "314";     // WHOWAS user information
    public const string RPL_ENDOFWHO = "315";       // End of WHO list
    public const string RPL_WHOISIDLE = "317";      // WHOIS idle time
    public const string RPL_ENDOFWHOIS = "318";     // End of WHOIS
    public const string RPL_WHOISCHANNELS = "319";  // WHOIS channels
    public const string RPL_LIST = "322";           // Channel list
    public const string RPL_LISTEND = "323";        // End of channel list
    public const string RPL_CHANNELMODEIS = "324";  // Channel mode
    public const string RPL_CREATIONTIME = "329";   // Channel creation time
    public const string RPL_NOTOPIC = "331";        // No topic is set
    public const string RPL_TOPIC = "332";          // Channel topic
    public const string RPL_TOPICWHOTIME = "333";   // Topic set by and when
    public const string RPL_INVITING = "341";       // Inviting user to channel
    public const string RPL_INVITELIST = "346";     // Invite exception list
    public const string RPL_ENDOFINVITELIST = "347"; // End of invite exception list
    public const string RPL_EXCEPTLIST = "348";     // Exception list
    public const string RPL_ENDOFEXCEPTLIST = "349"; // End of exception list
    public const string RPL_VERSION = "351";        // Server version
    public const string RPL_WHOREPLY = "352";       // WHO reply
    public const string RPL_NAMREPLY = "353";       // List of users in a channel
    public const string RPL_ENDOFNAMES = "366";     // End of names list
    public const string RPL_BANLIST = "367";        // Ban list
    public const string RPL_ENDOFBANLIST = "368";   // End of ban list
    public const string RPL_ENDOFWHOWAS = "369";    // End of WHOWAS
    public const string RPL_MOTDSTART = "375";      // Start of MOTD
    public const string RPL_MOTD = "372";           // Line of MOTD
    public const string RPL_ENDOFMOTD = "376";      // End of MOTD
    public const string RPL_YOUREOPER = "381";      // Confirmation of oper status
    public const string RPL_TIME = "391";           // Server time

    // 400 - 499: Error Replies
    public const string ERR_NOSUCHNICK = "401";     // No such nick
    public const string ERR_NOSUCHSERVER = "402";   // No such server
    public const string ERR_NOSUCHCHANNEL = "403";  // No such channel
    public const string ERR_CANNOTSENDTOCHAN = "404"; // Cannot send to channel
    public const string ERR_TOOMANYCHANNELS = "405"; // Too many channels
    public const string ERR_WASNOSUCHNICK = "406";  // Was no such nick
    public const string ERR_TOOMANYTARGETS = "407"; // Too many targets
    public const string ERR_NOSUCHSERVICE = "408";  // No such service
    public const string ERR_NOORIGIN = "409";       // No origin
    public const string ERR_INVALIDCAPCMD = "410";  // Invalid CAP command
    public const string ERR_NORECIPIENT = "411";    // No recipient given
    public const string ERR_NOTEXTTOSEND = "412";   // No text to send
    public const string ERR_NOTOPLEVEL = "413";     // No toplevel domain specified
    public const string ERR_WILDTOPLEVEL = "414";   // Wildcard in toplevel domain
    public const string ERR_UNKNOWNCOMMAND = "421"; // Unknown command
    public const string ERR_NOMOTD = "422";         // No MOTD available
    public const string ERR_FILEERROR = "424";      // File operation error
    public const string ERR_NONICKNAMEGIVEN = "431"; // No nickname given
    public const string ERR_ERRONEUSNICKNAME = "432"; // Erroneous nickname
    public const string ERR_NICKNAMEINUSE = "433";  // Nickname is already in use
    public const string ERR_NICKCOLLISION = "436";  // Nickname collision
    public const string ERR_UNAVAILRESOURCE = "437"; // Resource unavailable
    public const string ERR_USERNOTINCHANNEL = "441"; // User not in channel
    public const string ERR_NOTONCHANNEL = "442";   // Not on channel
    public const string ERR_USERONCHANNEL = "443";  // User already on channel
    public const string ERR_NOTREGISTERED = "451";  // Not registered yet
    public const string ERR_NEEDMOREPARAMS = "461"; // Need more parameters
    public const string ERR_ALREADYREGISTERED = "462"; // Already registered
    public const string ERR_PASSWDMISMATCH = "464"; // Password mismatch
    public const string ERR_YOUREBANNEDCREEP = "465"; // You are banned
    public const string ERR_CHANNELISFULL = "471";  // Channel is full
    public const string ERR_INVITEONLYCHAN = "473"; // Invite-only channel
    public const string ERR_BANNEDFROMCHAN = "474"; // Banned from channel
    public const string ERR_BADCHANNELKEY = "475";  // Bad channel key
    public const string ERR_NOPRIVILEGES = "481";   // Insufficient privileges
    public const string ERR_CHANOPRIVSNEEDED = "482"; // Channel operator privileges needed
    public const string ERR_CANTKILLSERVER = "483"; // Cannot kill a server
    public const string ERR_NOOPERHOST = "491";     // No oper host

    // 500 - 599: Server Errors
    public const string ERR_UNKNOWNERROR = "500";   // Unknown error
    public const string ERR_UMODEUNKNOWNFLAG = "501"; // Mode unknown flag
    public const string ERR_USERSDONTMATCH = "502"; // Users don't match

    // IRCv3 Extensions
    public const string RPL_MONONLINE = "730";      // Monitored user is online (IRCv3 MONITOR)
    public const string RPL_MONOFFLINE = "731";     // Monitored user is offline (IRCv3 MONITOR)
}
