using spennyIRC.Core;
using spennyIRC.Scripting.Helpers;

namespace spennyIRC.Scripting
{
    public class IrcCommandsBinder(IIrcCommands commands) : IIrcRuntimeBinder
    {
        private static readonly IrcCommandFactory cmdfactory = new();

        public void Bind()
        {
            // Regular commands
            commands.AddCommand("server", cmdfactory.CreateCommand("server", IrcCommandHelpers.ConnectServerAsync));
            commands.AddCommand("voice", cmdfactory.CreateCommand("voice", IrcCommandHelpers.VoiceAsync));
            commands.AddCommand("join", cmdfactory.CreateCommand("join", IrcCommandHelpers.JoinAsync));
            commands.AddCommand("rejoin", cmdfactory.CreateCommand("rejoin", IrcCommandHelpers.RejoinAsync));
            commands.AddCommand("part", cmdfactory.CreateCommand("part", IrcCommandHelpers.PartAsync));
            commands.AddCommand("nick", cmdfactory.CreateCommand("nick", IrcCommandHelpers.NickAsync));
            commands.AddCommand("quit", cmdfactory.CreateCommand("quit", IrcCommandHelpers.QuitAsync));
            commands.AddCommand("raw", cmdfactory.CreateCommand("raw", IrcCommandHelpers.RawAsync));
            commands.AddCommand("mode", cmdfactory.CreateCommand("mode", IrcCommandHelpers.ModeAsync));
            commands.AddCommand("msg", cmdfactory.CreateCommand("msg", IrcCommandHelpers.MsgAsync));
            commands.AddCommand("say", cmdfactory.CreateCommand("say", IrcCommandHelpers.SayAsync));
            commands.AddCommand("me", cmdfactory.CreateCommand("me", IrcCommandHelpers.MeAsync));
            commands.AddCommand("emote", cmdfactory.CreateCommand("emote", IrcCommandHelpers.MeAsync));
            commands.AddCommand("notice", cmdfactory.CreateCommand("notice", IrcCommandHelpers.NoticeAsync));
            commands.AddCommand("ctcp", cmdfactory.CreateCommand("ctcp", IrcCommandHelpers.CtcpAsync));
            commands.AddCommand("kick", cmdfactory.CreateCommand("kick", IrcCommandHelpers.KickAsync));
            commands.AddCommand("ban", cmdfactory.CreateCommand("ban", IrcCommandHelpers.BanAsync));
            commands.AddCommand("unban", cmdfactory.CreateCommand("unban", IrcCommandHelpers.UnbanAsync));
            commands.AddCommand("topic", cmdfactory.CreateCommand("topic", IrcCommandHelpers.TopicAsync));
            commands.AddCommand("names", cmdfactory.CreateCommand("names", IrcCommandHelpers.NamesAsync));
            commands.AddCommand("list", cmdfactory.CreateCommand("list", IrcCommandHelpers.ListAsync));
            commands.AddCommand("whois", cmdfactory.CreateCommand("whois", IrcCommandHelpers.WhoisAsync));
            commands.AddCommand("who", cmdfactory.CreateCommand("who", IrcCommandHelpers.WhoisAsync));

            // Debug commands
            commands.AddCommand("session", cmdfactory.CreateCommand("session", IrcCommandHelpers.GetSessionInfoAsync));
            commands.AddCommand("resetinfo", cmdfactory.CreateCommand("resetinfo", IrcCommandHelpers.ResetInfoAsync));
        }
    }
}
