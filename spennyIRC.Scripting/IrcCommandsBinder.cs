using spennyIRC.Scripting.Helpers;

namespace spennyIRC.Scripting
{
    public class IrcCommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
    {
        public void Bind()
        {
            AddCommand("server", BuiltInIrcCommands.ConnectServerAsync);
            AddCommand("voice", BuiltInIrcCommands.VoiceAsync);
            AddCommand("join", BuiltInIrcCommands.JoinAsync);
            AddCommand("rejoin", BuiltInIrcCommands.RejoinAsync);
            AddCommand("part", BuiltInIrcCommands.PartAsync);
            AddCommand("nick", BuiltInIrcCommands.NickAsync);
            AddCommand("quit", BuiltInIrcCommands.QuitAsync);
            AddCommand("raw", BuiltInIrcCommands.RawAsync);
            AddCommand("mode", BuiltInIrcCommands.ModeAsync);
            AddCommand("msg", BuiltInIrcCommands.MsgAsync);
            AddCommand("say", BuiltInIrcCommands.SayAsync);
            AddCommand("me", BuiltInIrcCommands.MeAsync);
            AddCommand("emote", BuiltInIrcCommands.MeAsync);
            AddCommand("notice", BuiltInIrcCommands.NoticeAsync);
            AddCommand("ctcp", BuiltInIrcCommands.CtcpAsync);
            AddCommand("kick", BuiltInIrcCommands.KickAsync);
            AddCommand("ban", BuiltInIrcCommands.BanAsync);
            AddCommand("unban", BuiltInIrcCommands.UnbanAsync);
            AddCommand("topic", BuiltInIrcCommands.TopicAsync);
            AddCommand("names", BuiltInIrcCommands.NamesAsync);
            AddCommand("list", BuiltInIrcCommands.ListAsync);
            AddCommand("whois", BuiltInIrcCommands.WhoisAsync);
            AddCommand("randnick", BuiltInIrcCommands.RandNickAsync);
            AddCommand("who", BuiltInIrcCommands.WhoAsync);
            AddCommand("session", BuiltInIrcCommands.GetSessionInfoAsync);
            AddCommand("resetinfo", BuiltInIrcCommands.ResetInfoAsync);
        }
    }
}
