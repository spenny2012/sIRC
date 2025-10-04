using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Tests;

[TestClass]
public class IrcClientTests
{
    [TestMethod]
    public void TestConnection()
    {
        string nick = "s" + MiscHelpers.GenerateRandomString(7);
        string ident = MiscHelpers.GenerateRandomString(4);
        string realName = MiscHelpers.GenerateRandomString(4);

        IrcClient ircClient = new();

        ircClient.ConnectAsync("irc.libera.chat", 6697, true).GetAwaiter().GetResult();

        Task.Delay(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult();

        ircClient.SendMessageAsync($"NICK {nick}").GetAwaiter().GetResult();
        ircClient.SendMessageAsync($"USER {ident} * * :{realName}").GetAwaiter().GetResult();

        while (true)
        {
            Task.Delay(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
        }
    }
}