using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Constants;
using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Core;

public class ClientRuntimeBinder(IIrcEvents events, IIrcServer server, IIrcLocalUser localUser) : IIrcRuntimeBinder
{
    public void Bind()
    {
        events.AddEvent(ProtocolNumericConstants.RPL_WELCOME, (ctx) => // 001 - Welcome
        {
            // :ircnet.hostsailor.com 001 sIK90lR :Welcome to the Internet Relay Network sIK90lR!~atEVT@185.202.221.94
            server.Connected = true;
            server.Host = ctx.LineParts[0][1..];
            return Task.CompletedTask;
        });
        events.AddEvent(ProtocolNumericConstants.RPL_YOURHOST, static (ctx) => { return Task.CompletedTask; }); // 002 - Your Host
        events.AddEvent(ProtocolNumericConstants.ERR_NICKNAMEINUSE, async (ctx) => // 433 - Nickname in use
        {
            // if not connected, change nick and store new credentials
            if (!server.Connected)
            {
                // if nick is already changed to nick2 and is also invalid
                if (localUser.Nick == localUser.Nick2)
                {
                    return;
                }
                // update local user nick
                localUser.Nick = localUser.Nick2;

                // change nick
                await ctx.IrcClient.SendMessageAsync($"NICK {localUser.Nick2}");
            }
        });
        events.AddEvent(ProtocolNumericConstants.RPL_ISUPPORT, (ctx) => // 005 - I Support
        {
            Dictionary<string, string> settings = ctx.Line.ExtractNetworkSettingsFrom005();

            if (settings.TryGetValue("NETWORK", out string? network))
                server.Network = network;

            foreach (KeyValuePair<string, string> setting in settings)
                server.Settings[setting.Key] = setting.Value;

            return Task.CompletedTask;
        });
        events.AddEvent(ProtocolNumericConstants.RPL_NOWAWAY, (ctx) => // 306 - Away
        {
            localUser.Away = true;
            return Task.CompletedTask;
        });
        events.AddEvent(ProtocolNumericConstants.RPL_UNAWAY, (ctx) => // 305 - Unaway
        {
            localUser.Away = false;
            return Task.CompletedTask;
        });
        events.AddEvent("NICK", (ctx) =>
        {
            if (ctx.Nick == localUser.Nick)
                localUser.Nick = ctx.Trailing!;
            return Task.CompletedTask;
        });
        events.AddEvent("JOIN", (ctx) =>
        {
            if (ctx.Nick == localUser.Nick)
            {
                string channelName = ctx.Recipient.TrimStart(':');
                IrcChannel channel = new() { Name = channelName };
                localUser.Channels.Add(channelName, channel);
            }

            return Task.CompletedTask;
        });
        events.AddEvent("PART", (ctx) =>
        {
            if (ctx.Nick == localUser.Nick)
                localUser.Channels.Remove(ctx.Recipient.TrimStart(':'));
            return Task.CompletedTask;
        });
        events.AddEvent("KICK", (ctx) =>
        {
            bool localUserKicked = ctx.LineParts[3] == localUser.Nick;
            if (localUserKicked)
                localUser.Channels.Remove(ctx.Recipient.TrimStart(':'));
            return Task.CompletedTask;
        });
        events.AddEvent("MODE", (ctx) =>
        {
            // TODO: handle modes
            return Task.CompletedTask;
        });
        events.AddEvent("DISCONNECT", (ctx) =>
        {
            localUser.Away = false;
            server.Clear();

            return Task.CompletedTask;
        });
        events.AddEvent("VERSION", static async (ctx) =>
        {
            await ctx.IrcClient.SendMessageAsync($"NOTICE {ctx.Nick} :\u0001[sIRC] v0.1\u0001");
        });
    }
}

/*
Fields to monitor:
    * 001 RPL_WELCOME (server info)       |   :irc.atw-inter.net 001 YourNick :Welcome to the Internet Relay Network YourNick!~Hello@45.13.235.55
    * 002 RPL_YOURHOST                    |   :irc.atw-inter.net 002 YourNick :Your host is irc.atw-inter.net, running version 2.11.2p3+0PNv1.06
    * 004 RPL_MYINFO                      |   :irc.atw-inter.net 004 YourNick irc.atw-inter.net 2.11.2p3+0PNv1.06 aoOirw abeiIklmnoOpqrRstv
    * 005 RPL_ISUPPORT (RFC2812 parsing)  |   :irc.atw-inter.net 005 YourNick RFC2812 PREFIX=(ov)@+ CHANTYPES=#&!+ MODES=3 CHANLIMIT=#&!+:42 NICKLEN=15 TOPICLEN=255 KICKLEN=255 MAXLIST=beIR:64 CHANNELLEN=50 IDCHAN=!:5 CHANMODES=beIR,k,l,imnpstaqrzZ :are supported by this server
    * 005 RPL_ISUPPORT (PENALTY parsing)  |   :irc.atw-inter.net 005 YourNick PENALTY FNC EXCEPTS=e INVEX=I CASEMAPPING=ascii NETWORK=IRCnet :are supported by this server
    * NICK (local user)                   |   :YourNick!~Hello@45.13.235.55 NICK :YourNick2
    * JOIN (local user)                   |   :YourNick!~Hello@45.13.235.55 JOIN :#test
    * MODE (local user)                   |   :YourNick MODE YourNick :+i
    * PART (local user)                   |
    * QUIT (local user)                   |   ERROR :Closing Link: YourNick[~Hello@45.13.235.55] ("TEST")
    * AWAY (local user)                   |
*/