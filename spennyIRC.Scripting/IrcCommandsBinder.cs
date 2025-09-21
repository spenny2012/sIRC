using spennyIRC.Scripting.Helpers;

namespace spennyIRC.Scripting
{
    public class IrcCommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
    {
        public void Bind()
        {
            AddCommand("ban", BuiltInIrcCommands.BanAsync);
            AddCommand("ctcp", BuiltInIrcCommands.CtcpAsync);
            AddCommand("emote", BuiltInIrcCommands.MeAsync);
            AddCommand("join", BuiltInIrcCommands.JoinAsync);
            AddCommand("kick", BuiltInIrcCommands.KickAsync);
            AddCommand("list", BuiltInIrcCommands.ListAsync);
            AddCommand("me", BuiltInIrcCommands.MeAsync);
            AddCommand("mode", BuiltInIrcCommands.ModeAsync);
            AddCommand("msg", BuiltInIrcCommands.MsgAsync);
            AddCommand("names", BuiltInIrcCommands.NamesAsync);
            AddCommand("nick", BuiltInIrcCommands.NickAsync);
            AddCommand("notice", BuiltInIrcCommands.NoticeAsync);
            AddCommand("part", BuiltInIrcCommands.PartAsync);
            //AddCommand("partall", BuiltInIrcCommands.PartAllChannelsAsync);
            AddCommand("quit", BuiltInIrcCommands.QuitAsync);
            AddCommand("randnick", BuiltInIrcCommands.RandNickAsync);
            AddCommand("raw", BuiltInIrcCommands.RawAsync);
            AddCommand("rejoin", BuiltInIrcCommands.RejoinAsync);
            AddCommand("resetinfo", BuiltInIrcCommands.ResetInfoAsync);
            AddCommand("say", BuiltInIrcCommands.SayAsync);
            AddCommand("server", BuiltInIrcCommands.ConnectServerAsync);
            AddCommand("session", BuiltInIrcCommands.GetSessionInfoAsync);
            AddCommand("topic", BuiltInIrcCommands.TopicAsync);
            AddCommand("unban", BuiltInIrcCommands.UnbanAsync);
            AddCommand("voice", BuiltInIrcCommands.VoiceAsync);
            AddCommand("who", BuiltInIrcCommands.WhoAsync);
            AddCommand("whois", BuiltInIrcCommands.WhoisAsync);
        }
    }
}