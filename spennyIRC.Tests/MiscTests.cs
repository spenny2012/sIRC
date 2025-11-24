using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Tests;

[TestClass]
public class MiscTests
{
    [TestMethod]
    public void TestMethod1()
    {
        IrcCommandInfo extractedInfo = CommandHelpers.ExtractCommandInfo("/msg test HELLO");
        string fromToken = "/msg test HELLO".GetTokenFrom(2);
    }
}