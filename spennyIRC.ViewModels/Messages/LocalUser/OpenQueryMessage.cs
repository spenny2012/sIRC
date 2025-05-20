namespace spennyIRC.ViewModels.Messages.LocalUser
{

    public class OpenQueryMessage(ISpennyIrcInstance Instance) : MessageBase(Instance)
    {
        public string Nick { get; set; } = string.Empty;
    }
}
