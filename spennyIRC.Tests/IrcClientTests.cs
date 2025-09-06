using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace spennyIRC.Tests;

[TestClass]
public class IrcClientTests
{
    [TestMethod]
    public void TestConnection()
    {
        var nick = "s" + MiscHelpers.GenerateRandomString(7);
        var ident = MiscHelpers.GenerateRandomString(4);
        var realName = MiscHelpers.GenerateRandomString(4);

        var ircClient = new IrcClient();

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
