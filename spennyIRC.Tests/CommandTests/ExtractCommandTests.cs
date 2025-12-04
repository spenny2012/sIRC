using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Tests.CommandTests;

[TestClass]
public class ExtractCommandTests
{
    const string MY_COMMAND = "/test  10    HELLO WORLD!";

    [TestMethod]
    public void ExtractCommandTest()
    {
        IrcCommandInfo cmd = MY_COMMAND.AsSpan(1).ToString().ExtractCommand();
        Assert.IsNotNull(cmd.Parameters);
    }

    [TestMethod]
    public void ExtractCommandParametersTest()
    {
        IrcCommandInfo cmd = MY_COMMAND.AsSpan(1).ToString().ExtractCommand();
        Assert.IsNotNull(cmd.Parameters);

        IrcCommandParametersInfo? prms = cmd.Parameters?.ExtractCommandParameters();
        Assert.IsNotNull(prms);
        Assert.IsNotNull(prms.Value.Parameters);

        int? getArgs = prms.Value.GetParam<int?>(0);
        Assert.AreEqual(10, getArgs);
    }
}
