using spennyIRC.Core.IRC.Constants;
using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Core.IRC;

public class IalRuntimeBinder(IIrcEvents events, IIrcInternalAddressList ial) : IIrcRuntimeBinder
{
    public void Bind() // TODO: finish adding whois info
    {
        events.AddEvent(ProtocolNumericConstants.RPL_MONONLINE, (ctx) => // 730 - Notify online
        {
            IrcExtractedUserInfo userInfo = ctx.Trailing!.ExtractBasicUserInfo();
            ial.InsertUser(userInfo);
            return Task.CompletedTask;
        });
        events.AddEvent(ProtocolNumericConstants.RPL_MONOFFLINE, (ctx) => // 731 - Notify offline
        {
            string nick = ctx.Trailing!;
            ial.RemoveNick(nick);
            return Task.CompletedTask;
        });
        events.AddEvent(ProtocolNumericConstants.RPL_WHOREPLY, (ctx) => // 352 - Who
        {
            IrcExtractedUserInfo info = ctx.LineParts.ExtractUserInfoFrom352();
            ial.UpsertUser(info, ctx.LineParts[3]);
            return Task.CompletedTask;
        });
        events.AddEvent(ProtocolNumericConstants.RPL_WHOISSERVER, (ctx) => // 312 - Whois
        {
            return Task.CompletedTask;
        });
        events.AddEvent(ProtocolNumericConstants.RPL_WHOISCHANNELS, (ctx) => // 319 - Whois Channels
        {
            return Task.CompletedTask;
        });

        events.AddEvent(ProtocolNumericConstants.RPL_NAMREPLY, (ctx) => // 353 - Names
        {
            IrcExtractedUserInfo[] users = [.. ctx.Trailing!.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => x.ExtractUserInfoFrom353())];
            ial.UpsertUsers(users, ctx.LineParts[4]);
            return Task.CompletedTask;
        });
        events.AddEvent("JOIN", (ctx) =>
        {
            IrcExtractedUserInfo info = ctx.LineParts[0][1..].ExtractBasicUserInfo();
            ial.UpsertChannel(info.Nick, ctx.Trailing!);
            return Task.CompletedTask;
        });
        events.AddEvent("PART", (ctx) =>
        {
            ial.RemoveChannel(ctx.Nick!, ctx.Recipient);
            return Task.CompletedTask;
        });
        events.AddEvent("NICK", (ctx) =>
        {
            ial.ChangeNick(ctx.Nick!, ctx.Trailing!);
            return Task.CompletedTask;
        });
        events.AddEvent("KICK", (ctx) =>
        {
            ial.RemoveChannel(ctx.Nick!, ctx.Recipient);
            return Task.CompletedTask;
        });
        events.AddEvent("MODE", (ctx) => // TODO: finish this, make this less awful
        {
            Dictionary<string, List<(bool isAdding, IrcChannelAccessType type)>> userUpdates = ctx.LineParts.ExtractChannelUserModes();
            foreach (KeyValuePair<string, List<(bool isAdding, IrcChannelAccessType type)>> user in userUpdates)
            {
                string nick = user.Key;
                foreach ((bool isAdding, IrcChannelAccessType type) in user.Value)
                {
                    if (isAdding)
                    {
                        // Add status
                    }
                    else
                    {
                        // Remove status
                    }
                }
            }
            // remove chan from user's IAL entry
            return Task.CompletedTask;
        });
        events.AddEvent("QUIT", (ctx) =>
        {
            ial.RemoveNick(ctx.Nick!);
            return Task.CompletedTask;
        });
        // TODO: Add WHOIS
    }
}

/*
Events to monitor:
* 353 (NAMES)    :shrimp.test.org 353 YourNick @ #ASDASDASD :YourNick ASDASDASD
* QUIT           :ZodBot!~ZodBot@90EE3181.D23D4749.DD274DCD.IP QUIT :Ping timeout: 240 seconds
* JOIN           :icrawl55!irc@1E5498DC.904CBC6.A1AB8D51.IP JOIN :#ASDASDASD
* PART           :ASDASDASD!~asdasdads@702C07FF.251B41EB.C2CFB607.IP PART #ASDASDASD :EMO-PART
* NICK           :ASDASDASD!~asdasdads@702C07FF.251B41EB.C2CFB607.IP NICK :ASDASDASD_
* PRIVMSG (PM)
* WHO (352)      :shrimp.test.org 352 YourNick #ASDASDASD ~Hello E9726DF6.B6B17E60.81B556F8.IP * YourNick H@ :0 Hi Guys
* WHOIS
* NOTIFY (NOTICE)

:shrimp.test.org 315 YourNick #ASDASDASD :End of /WHO list.
*/
/*
 311
 319
 312
 318
*/
//events.AddEvent("312", (ctx) => //RPL_WHOISOPERATOR
//{ 730, 731,
//    return Task.CompletedTask;
//});
//events.AddEvent("313", (ctx) => // RPL_WHOISIDLE
//{
//    return Task.CompletedTask;
//});
//events.AddEvent("317", (ctx) => // RPL_WHOISIDLE
//{
//    return Task.CompletedTask;
//});