using spennyIRC.Scripting.Helpers;

namespace spennyIRC.Scripting
{
    public class IrcCommandsBinder(IIrcCommands commands) : IrcCommandsBinderBase(commands)
    {
        public void Bind()
        {
            AddCommand("server", IrcCommandHelpers.ConnectServerAsync);
            AddCommand("voice", IrcCommandHelpers.VoiceAsync);
            AddCommand("join", IrcCommandHelpers.JoinAsync);
            AddCommand("rejoin", IrcCommandHelpers.RejoinAsync);
            AddCommand("part", IrcCommandHelpers.PartAsync);
            AddCommand("nick", IrcCommandHelpers.NickAsync);
            AddCommand("quit", IrcCommandHelpers.QuitAsync);
            AddCommand("raw", IrcCommandHelpers.RawAsync);
            AddCommand("mode", IrcCommandHelpers.ModeAsync);
            AddCommand("msg", IrcCommandHelpers.MsgAsync);
            AddCommand("say", IrcCommandHelpers.SayAsync);
            AddCommand("me", IrcCommandHelpers.MeAsync);
            AddCommand("emote", IrcCommandHelpers.MeAsync);
            AddCommand("notice", IrcCommandHelpers.NoticeAsync);
            AddCommand("ctcp", IrcCommandHelpers.CtcpAsync);
            AddCommand("kick", IrcCommandHelpers.KickAsync);
            AddCommand("ban", IrcCommandHelpers.BanAsync);
            AddCommand("unban", IrcCommandHelpers.UnbanAsync);
            AddCommand("topic", IrcCommandHelpers.TopicAsync);
            AddCommand("names", IrcCommandHelpers.NamesAsync);
            AddCommand("list", IrcCommandHelpers.ListAsync);
            AddCommand("whois", IrcCommandHelpers.WhoisAsync);
            AddCommand("who", IrcCommandHelpers.WhoAsync);
            AddCommand("clear", IrcCommandHelpers.ClearAsync);
            AddCommand("session", IrcCommandHelpers.GetSessionInfoAsync);
            AddCommand("resetinfo", IrcCommandHelpers.ResetInfoAsync);
        }
    }
}
