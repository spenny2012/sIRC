namespace spennyIRC.Core.IRC.Constants
{
    public class IrcClientConstants
    {
#if RELEASE
        public static string VERSION_REPLY = "sIRC v0.5";
#endif
#if DEBUG
        public static string VERSION_REPLY = "Bleh";
#endif

    }
}
