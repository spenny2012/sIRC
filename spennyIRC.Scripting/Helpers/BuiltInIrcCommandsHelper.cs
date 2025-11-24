using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using spennyIRC.Scripting.Attributes;
using spennyIRC.Scripting.Helpers.UrbanDictionary;
using System.Diagnostics;

namespace spennyIRC.Scripting.Helpers;

[IrcCommandClass("Basic Commands")]
public static class BuiltInIrcCommandsHelper
{
    private const int DEFAULT_IRC_PORT = 6697;

    [IrcCommand("looks up a word from the dictionary (dict.org)")]
    public static async Task DictAsync(string parameters, IIrcSession session)
    {
        session.WindowService.DoEcho(session.ActiveWindow, $"-\r\n{await DictLookupHelper.DefineAsync(parameters)}-\r\n");
    }

    [IrcCommand("looks up a slang term from the UrbanDictionary")]
    public static async Task UdAsync(string parameters, IIrcSession session)
    {
        // TODO: find wrapped quotes, if no quotes, parse normally
        List<UdDefinition> foundDefinition = await UdLookupHelper.UdLookupAsync(parameters);

        if (foundDefinition.Count > 0)
        {
            UdDefinition firstDefinition = foundDefinition.First();
            PrintPropertiesHelper.BasicPrintProperties(firstDefinition, session);
        }
        else
        {
            session.WindowService.DoEcho(session.ActiveWindow, $"-\r\nNo definitions found for '{parameters}'\r\n-");
        }
    }

    [IrcCommand("bans a user")]
    public static async Task BanAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (paramParts.Length >= 1)
        {
            await session.Client.SendMessageAsync($"MODE {paramParts[0]} +b :{paramParts[1]}");
        }
    }

    [IrcCommand("connect to a server")]
    public static async Task ServerAsync(string serverInfo, IIrcSession session)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(serverInfo);

            if (session.Server.Connected)
            {
                await session.ClientManager.QuitAsync();
            }

            session.WindowService.Echo("Status", $"*** Connecting to {serverInfo}...");

            string[]? paramsParts = serverInfo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string server = paramsParts![0];
            if (paramsParts == null || paramsParts.Length < 2)
            {
                session.Server.Host = server;
                session.Server.Port = $"+{DEFAULT_IRC_PORT}";
                session.Server.IsTls = true;
                await session.ClientManager.ConnectAsync(server, DEFAULT_IRC_PORT, true);
                return;
            }

            string specifiedPort = paramsParts[1];
            bool useSsl = specifiedPort.StartsWith('+');
            if (int.TryParse(useSsl ? specifiedPort[1..] : specifiedPort, out int newPort))
            {
                session.Server.Host = server;
                session.Server.Port = specifiedPort;
                session.Server.IsTls = useSsl;
                await session.ClientManager.ConnectAsync(paramsParts[0], newPort, useSsl);
                return;
            }
        }
        catch (Exception e)
        {
#if DEBUG
            session.WindowService.Echo("Status", $"ERROR: {e}");
            Debug.WriteLine($"ERROR: {e}");
#endif
#if RELEASE
            session.WindowService.Echo("Status", $"ERROR: {e.Message}");
#endif
            session.Server.Clear();
        }
    }

    [IrcCommand("performs a ctcp on a user")]
    public static async Task CtcpAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (paramParts.Length < 2)
        {
            return;
        }

        string target = paramParts[0];
        string command = paramParts[1].ToUpper();
        string ctcpMessage;

        if (paramParts.Length > 2)
        {
            string commandParams = parameters.GetTokenFrom(2);
            ctcpMessage = $"\u0001{command} {commandParams}\u0001";
        }
        else
        {
            ctcpMessage = $"\u0001{command}\u0001";
        }

        string message = $"PRIVMSG {target} :{ctcpMessage}";
        await session.Client.SendMessageAsync(message);
    }

    [IrcCommand("for debug purposes.  shows user session info")]
    public static Task SessionInfoAsync(string parameters, IIrcSession session)
    {
        session.WindowService.Echo(session.ActiveWindow, "-");

        PrintPropertiesHelper.PrintProperties(session.LocalUser, session);
        PrintPropertiesHelper.PrintProperties(session.Server, session);

        session.WindowService.Echo(session.ActiveWindow, "-");

        return Task.CompletedTask;
    }

    //public static Task PartAllChannelsAsync(string parameters, IIrcSession session)
    //{
    //    session.Client.SendMessageAsync("JOIN #0,0");
    //    return Task.CompletedTask;
    //}

    public static async Task IalLookupAsync(string parameters, IIrcSession session)
    {
        IIrcInternalAddressList ial = session.Ial;
    }

    public static bool IsConnected(IIrcSession session)
    {
        if (!session.Server.Connected)
        {
            session.WindowService.Echo(session.ActiveWindow, $"*** Not connected to server");
        }
        return session.Server.Connected;
    }

    [IrcCommand("joins a channel")]
    public static async Task JoinAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"JOIN {parameters}");
    }

    [IrcCommand("kicks a user")]
    public static async Task KickAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (paramParts.Length >= 2)
        {
            await session.Client.SendMessageAsync($"KICK {paramParts[0]} {paramParts[1]} :{parameters.GetTokenFrom(2)}");
        }
        else if (paramParts.Length == 1)
        {
            await session.Client.SendMessageAsync($"KICK {paramParts[0]} {paramParts[1]} :Kicked");
        }
    }

    [IrcCommand("lists channels")]
    public static async Task ListAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"LIST {parameters}");
    }

    [IrcCommand("performs an emote")]
    public static async Task MeAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"PRIVMSG {session.ActiveWindow} :\u0001ACTION {parameters}\u0001");
        session.WindowService.Echo(session.ActiveWindow, $"* {session.LocalUser.Nick} {parameters}");
    }

    public static Task ModeAsync(string parameters, IIrcSession session)
    {
        // Implement mode command logic here
        return Task.CompletedTask;
    }

    [IrcCommand("sends a user or a channel a message")]
    public static async Task MsgAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string msg = parameters.GetTokenFrom(1);

        await session.Client.SendMessageAsync($"PRIVMSG {paramParts[0]} :{msg}");
        session.WindowService.Echo(session.ActiveWindow, $"*{paramParts[0]}* {msg}");
    }

    [IrcCommand("performs names")]
    public static async Task NamesAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"NAMES {parameters}");
    }

    [IrcCommand("changes nick")]
    public static async Task NickAsync(string parameters, IIrcSession session)
    {
        if (!session.Server.Connected)
        {
            string nick = session.LocalUser.Nick = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            session.WindowService.Echo(session.ActiveWindow, $"*** You are now known as {nick}");
            return;
        }

        await session.Client.SendMessageAsync($"NICK {parameters}");
    }

    [IrcCommand("sends a notice to a user or a channel")]
    public static async Task NoticeAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string msg = parameters.GetTokenFrom(1);

        await session.Client.SendMessageAsync($"NOTICE {paramParts[0]} :{msg}");
        session.WindowService.Echo(session.ActiveWindow, $"-{paramParts[0]}- {msg}");
    }

    [IrcCommand("parts a channel")]
    public static async Task PartAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string partMsg = parameters.GetTokenFrom(1);
        await session.Client.SendMessageAsync($"PART {paramParts[0]} :{partMsg}");
    }

    [IrcCommand("quits a server")]
    public static async Task QuitAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"QUIT :{parameters}");
    }

    [IrcCommand("changes nick to a random one")]
    public static async Task RandNickAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"NICK s{MiscHelpers.GenerateRandomString(Random.Shared.Next(3, 8))}");
    }

    [IrcCommand("sends a raw message to the current IRC server")]
    public static async Task RawAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync(parameters);
    }

    [IrcCommand("rejoins a channel")]
    public static async Task RejoinAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        session.WindowService.Echo(session.ActiveWindow, $"Rejoining {session.ActiveWindow}...");
        await session.Client.SendMessageAsync($"PART {session.ActiveWindow}\r\nJOIN {session.ActiveWindow}");
    }

    [IrcCommand("generates new user information")]
    public static Task ResetInfoAsync(string parameters, IIrcSession session)
    {
        session.LocalUser.Nick = "s" + MiscHelpers.GenerateRandomString(7);
        session.LocalUser.Nick2 = "s" + MiscHelpers.GenerateRandomString(7);
        session.LocalUser.Ident = MiscHelpers.GenerateRandomString(Random.Shared.Next(3, 8));
        session.LocalUser.Realname = MiscHelpers.GenerateRandomString(Random.Shared.Next(5, 16));

        session.WindowService.Echo(session.ActiveWindow, $"Reset user info:\r\n Nick: {session.LocalUser.Nick}\r\n Ident: {session.LocalUser.Ident}\r\n Real Name: {session.LocalUser.Realname}");

        return Task.CompletedTask;
    }

    [IrcCommand("messages the active window")]
    public static async Task SayAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"PRIVMSG {session.ActiveWindow} :{parameters}");
        session.WindowService.Echo(session.ActiveWindow, $"[{session.LocalUser.Nick}] {parameters}");
    }

    [IrcCommand("changes topic of a channel")]
    public static async Task TopicAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (paramParts.Length >= 1)
        {
            await session.Client.SendMessageAsync($"TOPIC {paramParts[0]} :{parameters.GetTokenFrom(1)}");
        }
    }

    [IrcCommand("unbans a user from a channel")]
    public static async Task UnbanAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (paramParts.Length >= 1)
        {
            await session.Client.SendMessageAsync($"MODE {paramParts[0]} -b :{paramParts[1]}");
        }
    }

    [IrcCommand("voices a user in a specified channel")]
    public static async Task VoiceAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        await session.Client.SendMessageAsync($"MODE {paramParts[0]} +v {paramParts[1]}");
    }

    [IrcCommand("performs a who")]
    public static async Task WhoAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"WHO {parameters}");
    }

    [IrcCommand("performs a whois")]
    public static async Task WhoisAsync(string parameters, IIrcSession session)
    {
        if (!IsConnected(session)) return;

        await session.Client.SendMessageAsync($"WHOIS {parameters}");
    }

    [IrcCommand("sets local user's ident (user must reconnect to any active servers for changes to take effect)")]
    public static Task SetIdentAsync(string parameters, IIrcSession session)
    {
        if (parameters == null) return Task.CompletedTask;
        session.WindowService.Echo(session.ActiveWindow, $"Ident set to: {session.LocalUser.Ident = parameters.Split(' ')[0]}");
        return Task.CompletedTask;
    }

    [IrcCommand("sets local user's realname (user must reconnect to any active servers for changes to take effect)")]
    public static Task SetRealNameAsync(string parameters, IIrcSession session)
    {
        if (parameters == null) return Task.CompletedTask;

        session.WindowService.Echo(session.ActiveWindow, $"Real name set to: {session.LocalUser.Realname = parameters}");

        return Task.CompletedTask;
    }
}