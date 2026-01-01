using CommunityToolkit.Mvvm.Messaging;
using spennyIRC.Core;
using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Constants;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.ViewModels.Messages;
using spennyIRC.ViewModels.Messages.Channel;
using spennyIRC.ViewModels.Messages.LocalUser;
using spennyIRC.ViewModels.Messages.Server;
using spennyIRC.ViewModels.Messages.Window;

namespace spennyIRC.ViewModels;

public class ViewModelRuntimeBinder(IIrcSession session) : IIrcRuntimeBinder
{
    private const string StatusWindow = "Status";
    private const string AllWindows = "All";
    private readonly IIrcLocalUser _user = session.LocalUser;
    private readonly IWindowService _echoSvc = session.WindowService;
    private readonly IIrcEvents _events = session.Events;
    private readonly IIrcServer _server = session.Server;

    public void Bind()
    {
        // TODO: add message for disconnect to clear all names from channels
        _events.AddEvent("DISCONNECT", (ctx) =>
        {
            WeakReferenceMessenger.Default.Send(new ServerDisconnectedMessage(session));
            _echoSvc.Echo(AllWindows, $"* Disconnected: {ctx.Line}");
            return Task.CompletedTask;
        });
        _events.AddEvent("TOPIC", (ctx) =>
        {
            WeakReferenceMessenger.Default.Send(new ChannelTopicChangeMessage(session) { Channel = ctx.Recipient, Topic = ctx.Trailing! });
            _echoSvc.Echo(ctx.LineParts[2], $"{ctx.Nick} changed the subject to: {ctx.Trailing}");
            return Task.CompletedTask;
        });
        _events.AddEvent("QUIT", (ctx) =>
        {
            WeakReferenceMessenger.Default.Send(new UserQuitMessage(session)
            {
                Nick = ctx.Nick!,
                Host = ctx.LineParts[0].ExtractFullHost().ToString(),
                Message = ctx.Trailing!,
            });
            return Task.CompletedTask;
        });
        _events.AddEvent("JOIN", (ctx) =>
        {
            string channel = ctx.Trailing!;
            if (ctx.Nick == _user.Nick)
            {
                WeakReferenceMessenger.Default.Send(new ChannelAddMessage(session) { Channel = channel! });
                return Task.CompletedTask;
            }
            WeakReferenceMessenger.Default.Send(new ChannelJoinMessage(session) { Channel = channel!, Nick = ctx.Nick! });
            _echoSvc.Echo(channel, $"»» {ctx.Nick} ({ctx.LineParts[0].ExtractFullHost()}) joined {channel}");
            return Task.CompletedTask;
        });
        _events.AddEvent("PART", (ctx) =>
        {
            string channel = ctx.Recipient!;
            WeakReferenceMessenger.Default.Send(new ChannelPartMessage(session)
            {
                Nick = ctx.Nick!,
                Host = ctx.LineParts[0].ExtractFullHost().ToString(),
                Channel = channel,
                Message = ctx.Trailing!
            });
            _echoSvc.Echo(channel, $"»» {ctx.Nick} ({ctx.LineParts[0].ExtractFullHost()}) parted {channel} ({ctx.Trailing})");
            return Task.CompletedTask;
        });
        _events.AddEvent("KICK", (ctx) =>
        {
            var kickedNick = ctx.LineParts[3];
            
            WeakReferenceMessenger.Default.Send(new ChannelKickMessage(session)
            {
                Nick = ctx.Nick!,
                KickedNick = kickedNick,
                Channel = ctx.Recipient,
                Message = ctx.Trailing!
            });
            string text = kickedNick == _user.Nick ? "You were" : $"{kickedNick} was";
            _echoSvc.Echo(ctx.Recipient, $"»» {text} kicked from {ctx.Recipient} by {ctx.Nick} ({ctx.Trailing})");
            return Task.CompletedTask;
        });
        _events.AddEvent("MODE", (ctx) =>
        {
            string channel = ctx.LineParts[2];
            string? modes = ctx.Trailing;
            return Task.CompletedTask;
        });
        _events.AddEvent("NOTICE", (ctx) =>
        {
            _echoSvc.Echo(StatusWindow, $"-{ctx.Nick}- {ctx.Trailing}");
            return Task.CompletedTask;
        });
        _events.AddEvent("PRIVMSG", (ctx) =>
        {
            bool isDm = ctx.Recipient == _user.Nick;
            if (isDm) // handle query
            {
                WeakReferenceMessenger.Default.Send(new ServerOpenedQueryMessage(session)
                {
                    Nick = ctx.Nick!,
                    Message = ctx.Trailing!
                });
            }
            _echoSvc.Echo(isDm ? ctx.Nick! : ctx.Recipient, $"[{ctx.Nick}] {ctx.Trailing}");
            return Task.CompletedTask;
        });
        _events.AddEvent("NICK", (ctx) =>
        {
            if (ctx.Trailing == _user.Nick)
            {
                _echoSvc.Echo(AllWindows, $"*** You are now known as {ctx.Trailing}");

                WeakReferenceMessenger.Default.Send(new LocalUserNickChangeMessage(session)
                {
                    Nick = ctx.Nick!,
                    NewNick = ctx.Trailing
                });

                return Task.CompletedTask;
            }
            WeakReferenceMessenger.Default.Send(new NickChangedMessage(session)
            {
                Nick = ctx.Nick!,
                NewNick = ctx.Trailing!
            });

            return Task.CompletedTask;
            // TODO: finish adding this logic (requires comchans)
        });

        // 001 - 099: Basic Replies (Welcome and Server Info)
        _events.AddEvent(ProtocolNumericConstants.RPL_WELCOME, (ctx) => // 001 - Welcome message
        {
            string id = ctx.LineParts[0][1..];
            _server.NetworkId = id;
            _echoSvc.Echo(StatusWindow, $"*** Connected to {id}");
            return Task.CompletedTask;
        });
        _events.AddEvent(ProtocolNumericConstants.RPL_YOURHOST, (ctx) => // 002 - Your host
        {
            _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3));
            return Task.CompletedTask;
        });
        _events.AddEvent(ProtocolNumericConstants.RPL_CREATED, (ctx) => // 003 - Server created message
        {
            _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3));
            return Task.CompletedTask;
        });
        _events.AddEvent(ProtocolNumericConstants.RPL_ISUPPORT, (ctx) => // 005 - ISUPPORT information
        {
            WeakReferenceMessenger.Default.Send(new ServerISupportMessage(session) { });
            _echoSvc.Echo(StatusWindow, $"{ctx.Line.GetTokenFrom(3)}");
            return Task.CompletedTask;
        });
        _events.AddEvent(ProtocolNumericConstants.PROCESSING_REQUEST, (ctx) => // 020 - Processing request message
        {
            _echoSvc.Echo(StatusWindow, ctx.Trailing!);
            return Task.CompletedTask;
        });
        _events.AddEvent(ProtocolNumericConstants.RPL_TOPIC, (ctx) => // 332 - Channel topic
        {
            string channel = ctx.LineParts[3];
            WeakReferenceMessenger.Default.Send(new ChannelTopicMessage(session) { Channel = channel, Topic = ctx.Trailing! });
            _echoSvc.Echo(ctx.LineParts[3], $"Topic: {ctx.Trailing}");
            return Task.CompletedTask;
        });
        _events.AddEvent(ProtocolNumericConstants.RPL_TOPICWHOTIME, (ctx) => // 333 - Topic set by and when
        {
            string channel = ctx.LineParts[3];
            string nick = ctx.LineParts[4];

            if (long.TryParse(ctx.LineParts[5], out long unixTimestamp))
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                DateTime dateTime = dateTimeOffset.LocalDateTime;
                _echoSvc.Echo(channel, $"Topic set by {nick} on {dateTime:g}");
            }
            return Task.CompletedTask;
        });
        _events.AddEvent(ProtocolNumericConstants.RPL_NAMREPLY, (ctx) => // 353 - List of users in a channel
        {
            string channel = ctx.LineParts[4];
            string[] nicks = ctx.Trailing!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            WeakReferenceMessenger.Default.Send(new ChannelAddNicksMessage(session) { Channel = channel, Nicks = nicks });
            return Task.CompletedTask;
        });
        _events.AddEvent(ProtocolNumericConstants.RPL_MYINFO, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));

        // 200 - 299: Trace and Stats Replies
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACELINK, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACECONNECTING, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACEHANDSHAKE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACEUNKNOWN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACEOPERATOR, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACEUSER, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACESERVER, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACESERVICE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACENEWTYPE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRACECLASS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_STATSLINKINFO, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_STATSCOMMANDS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_STATSCLINE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_STATSNLINE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_STATSILINE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_STATSKLINE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_STATSYLINE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFSTATS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_UMODEIS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_STATSCONN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_LUSERCLIENT, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_LUSEROP, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_LUSERUNKNOWN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_LUSERCHANNELS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_LUSERME, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ADMINME, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ADMINLOC1, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ADMINLOC2, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ADMINEMAIL, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TRYAGAIN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_LOCALUSERS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_GLOBALUSERS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));

        // 300 - 399: User and Channel Replies
        _events.AddEvent(ProtocolNumericConstants.RPL_AWAY, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_USERHOST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ISON, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_UNAWAY, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_NOWAWAY, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_WHOISUSER, async (ctx) => _echoSvc.Echo(session.ActiveWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_WHOISSERVER, async (ctx) => _echoSvc.Echo(session.ActiveWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_WHOISOPERATOR, async (ctx) => _echoSvc.Echo(session.ActiveWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_WHOWASUSER, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFWHO, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_WHOISIDLE, async (ctx) => _echoSvc.Echo(session.ActiveWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFWHOIS, async (ctx) => _echoSvc.Echo(session.ActiveWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_WHOISCHANNELS, async (ctx) => _echoSvc.Echo(session.ActiveWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_LIST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_LISTEND, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_CHANNELMODEIS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_CREATIONTIME, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_NOTOPIC, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_INVITING, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_INVITELIST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFINVITELIST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_EXCEPTLIST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFEXCEPTLIST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_VERSION, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_WHOREPLY, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_BANLIST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFBANLIST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFNAMES, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFWHOWAS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_MOTDSTART, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_MOTD, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_ENDOFMOTD, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_YOUREOPER, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_TIME, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));

        // 400 - 499: Error Replies
        _events.AddEvent(ProtocolNumericConstants.ERR_NOSUCHNICK, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOSUCHSERVER, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOSUCHCHANNEL, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_CANNOTSENDTOCHAN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_TOOMANYCHANNELS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_WASNOSUCHNICK, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_TOOMANYTARGETS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOSUCHSERVICE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOORIGIN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_INVALIDCAPCMD, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NORECIPIENT, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOTEXTTOSEND, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOTOPLEVEL, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_WILDTOPLEVEL, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_UNKNOWNCOMMAND, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOMOTD, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_FILEERROR, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NONICKNAMEGIVEN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_ERRONEUSNICKNAME, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NICKNAMEINUSE, (ctx) => { _echoSvc.Echo(StatusWindow, ctx.Trailing!); return Task.CompletedTask; });
        _events.AddEvent(ProtocolNumericConstants.ERR_NICKCOLLISION, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_UNAVAILRESOURCE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_USERNOTINCHANNEL, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOTONCHANNEL, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_USERONCHANNEL, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOTREGISTERED, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NEEDMOREPARAMS, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_ALREADYREGISTERED, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_PASSWDMISMATCH, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_YOUREBANNEDCREEP, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_CHANNELISFULL, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_INVITEONLYCHAN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_BANNEDFROMCHAN, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_BADCHANNELKEY, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOPRIVILEGES, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_CHANOPRIVSNEEDED, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_CANTKILLSERVER, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_NOOPERHOST, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));

        // 500 - 599: Server Errors
        _events.AddEvent(ProtocolNumericConstants.ERR_UNKNOWNERROR, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_UMODEUNKNOWNFLAG, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.ERR_USERSDONTMATCH, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));

        // 700+: IRCv3 Extensions
        _events.AddEvent(ProtocolNumericConstants.RPL_MONONLINE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
        _events.AddEvent(ProtocolNumericConstants.RPL_MONOFFLINE, async (ctx) => _echoSvc.Echo(StatusWindow, ctx.Line.GetTokenFrom(3)));
    }
}