using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Tests.CommandTests;

[TestClass]
public class ExtractCommandTests
{
    const string WORKING_COMMAND = "/test  10    HELLO      WORLD!";
    const string BROKEN_COMMAND = "/test    HELLO   WORLD!";

    [TestMethod]
    public void ExtractCommandTest()
    {
        IrcCommandInfo cmd = WORKING_COMMAND.AsSpan(1).ToString().ExtractCommand();
        Assert.IsNotNull(cmd.Parameters);
    }

    [TestMethod]
    public void ExtractWorkingParametersTest()
    {
        IrcCommandInfo cmd = WORKING_COMMAND.AsSpan(1).ToString().ExtractCommand();
        Assert.IsNotNull(cmd.Parameters);

        IrcCommandParametersInfo prms = cmd.ExtractCommandParameters();
        Assert.IsNotNull(prms.Parameters);

        // Test nullable int
        int? getArgs = prms.GetParam<int?>(0);
        Assert.AreEqual(10, getArgs);

        // Test regular int
        int? getArgs2 = prms.GetParam<int>(0);
        Assert.AreEqual(10, getArgs2);

        // Test nullable string
        string? getArgs3 = prms.GetParam<string?>(1);
        Assert.AreEqual("HELLO", getArgs3);
    }

    [TestMethod]
    public void ExtractBrokenParametersTest()
    {
        IrcCommandInfo brokenCmd = BROKEN_COMMAND.AsSpan(1).ToString().ExtractCommand();
        Assert.IsNotNull(brokenCmd.Parameters);

        IrcCommandParametersInfo prms = brokenCmd.ExtractCommandParameters();
        Assert.IsNotNull(prms.Parameters);

        // Test nullable int
        int? getArgs = prms.GetParam<int?>(0);
        Assert.IsNull(getArgs);

        // Test nullable string
        string? getArgs2 = prms.GetParam<string?>(0);
        Assert.AreEqual("HELLO", getArgs2);
    }
}
