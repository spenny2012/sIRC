using spennyIRC.Core.IRC;

namespace spennyIRC.ViewModels.Messages.LocalUser
{

    public class OpenQueryMessage(IIrcSession Instance) : MessageBase(Instance)
    {
        public string Nick { get; set; } = string.Empty;
    }
}
