using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using System.Diagnostics;
using System.Reflection;

namespace spennyIRC.Scripting.Helpers
{
    public static class BuiltInIrcCommands
    {
        private const int DEFAULT_IRC_PORT = 6697;

        public static async Task BanAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (paramParts.Length >= 1)
            {
                await session.Client.SendMessageAsync($"MODE {paramParts[0]} +b :{paramParts[1]}");
            }
        }

        public static async Task ConnectServerAsync(string serverInfo, IIrcSession session)
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(serverInfo);

                if (session.Server.Connected)
                {
                    await session.ClientManager.QuitAsync();
                }

                session.EchoService.Echo("Status", $"*** Connecting to {serverInfo}...");

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
                session.EchoService.Echo("Status", $"ERROR: {e}");
                Debug.WriteLine($"ERROR: {e}");
#endif
#if RELEASE
                session.EchoService.Echo("Status", $"ERROR: {e.Message}");
#endif
                session.Server.Clear();
            }
        }

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

        public static Task GetSessionInfoAsync(string parameters, IIrcSession session)
        {
            session.EchoService.Echo(session.ActiveWindow, "-");
            PrintClassProperties(session.LocalUser, session);
            PrintClassProperties(session.Server, session);
            session.EchoService.Echo(session.ActiveWindow, "-");

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
                session.EchoService.Echo(session.ActiveWindow, $"*** Not connected to server");
            }
            return session.Server.Connected;
        }

        public static async Task JoinAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"JOIN {parameters}");
        }

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

        public static async Task ListAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"LIST {parameters}");
        }

        public static async Task MeAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"PRIVMSG {session.ActiveWindow} :\u0001ACTION {parameters}");
            session.EchoService.Echo(session.ActiveWindow, $"* {session.LocalUser.Nick} {parameters}");
        }

        public static Task ModeAsync(string parameters, IIrcSession session)
        {
            // Implement mode command logic here
            return Task.CompletedTask;
        }

        public static async Task MsgAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string msg = parameters.GetTokenFrom(1);

            await session.Client.SendMessageAsync($"PRIVMSG {paramParts[0]} :{msg}");
            session.EchoService.Echo(session.ActiveWindow, $"*{paramParts[0]}* {msg}");
        }

        public static async Task NamesAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"NAMES {parameters}");
        }

        public static async Task NickAsync(string parameters, IIrcSession session)
        {
            if (!session.Server.Connected)
            {
                string nick = session.LocalUser.Nick = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                session.EchoService.Echo(session.ActiveWindow, $"*** You are now known as {nick}");
                return;
            }

            await session.Client.SendMessageAsync($"NICK {parameters}");
        }

        public static async Task NoticeAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string msg = parameters.GetTokenFrom(1);

            await session.Client.SendMessageAsync($"NOTICE {paramParts[0]} :{msg}");
            session.EchoService.Echo(session.ActiveWindow, $"-{paramParts[0]}- {msg}");
        }

        public static async Task PartAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string partMsg = parameters.GetTokenFrom(1);
            await session.Client.SendMessageAsync($"PART {paramParts[0]} :{partMsg}");
        }

        public static void PrintClassProperties(object obj, IIrcSession session)
        {
            Type type = obj.GetType();
            session.EchoService.Echo(session.ActiveWindow, $"Properties of {type.Name}:");

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                try
                {
                    object? value = property.GetValue(obj);
                    string? valueString = value != null ? value.ToString() : "null";
                    session.EchoService.Echo(session.ActiveWindow, $"{property.Name}: {valueString}");
                }
                catch (Exception ex)
                {
                    session.EchoService.Echo(session.ActiveWindow, $"{property.Name}: Error retrieving value ({ex.Message})");
                }
            }
        }

        public static async Task QuitAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"QUIT :{parameters}");
        }

        public static async Task RandNickAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"NICK s{MiscHelpers.GenerateRandomString(8)}");
        }

        public static async Task RawAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync(parameters);
        }

        public static async Task RejoinAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            session.EchoService.Echo(session.ActiveWindow, $"Rejoining {session.ActiveWindow}...");
            await session.Client.SendMessageAsync($"PART {session.ActiveWindow}\r\nJOIN {session.ActiveWindow}");
        }

        public static Task ResetInfoAsync(string parameters, IIrcSession session)
        {
            session.LocalUser.Nick = "s" + MiscHelpers.GenerateRandomString(7);
            session.LocalUser.Nick2 = "s" + MiscHelpers.GenerateRandomString(7);
            session.LocalUser.Ident = MiscHelpers.GenerateRandomString(5);
            session.LocalUser.Realname = MiscHelpers.GenerateRandomString(10);
            session.EchoService.Echo(session.ActiveWindow, $"Reset user info:\r\n Nick: {session.LocalUser.Nick}\r\n Ident: {session.LocalUser.Ident}\r\n Real Name: {session.LocalUser.Realname}");

            return Task.CompletedTask;
        }

        public static async Task SayAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"PRIVMSG {session.ActiveWindow} :{parameters}");
            session.EchoService.Echo(session.ActiveWindow, $"[{session.LocalUser.Nick}] {parameters}");
        }

        public static async Task TopicAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (paramParts.Length >= 1)
            {
                await session.Client.SendMessageAsync($"TOPIC {paramParts[0]} :{parameters.GetTokenFrom(1)}");
            }
        }

        public static async Task UnbanAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (paramParts.Length >= 1)
            {
                await session.Client.SendMessageAsync($"MODE {paramParts[0]} -b :{paramParts[1]}");
            }
        }

        public static async Task VoiceAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            string[] paramParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            await session.Client.SendMessageAsync($"MODE {paramParts[0]} +v {paramParts[1]}");
        }

        public static async Task WhoAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"WHO {parameters}");
        }

        public static async Task WhoisAsync(string parameters, IIrcSession session)
        {
            if (!IsConnected(session)) return;

            await session.Client.SendMessageAsync($"WHOIS {parameters}");
        }
    }
}