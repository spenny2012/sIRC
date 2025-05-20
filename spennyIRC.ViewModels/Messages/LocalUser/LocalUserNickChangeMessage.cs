namespace spennyIRC.ViewModels.Messages.LocalUser
{
    public class LocalUserNickChangeMessage(ISpennyIrcInstance instance) : MessageBase(instance)
    {
        public string Nick { get; set; } = string.Empty;
        public string NewNick { get; set; } = string.Empty;
    }
}
