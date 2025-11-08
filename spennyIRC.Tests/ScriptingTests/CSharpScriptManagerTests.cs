using Moq;
using spennyIRC.Scripting.Commands;
using spennyIRC.Scripting.Engine;

namespace spennyIRC.Tests.ScriptingTests;

[TestClass]
public class CSharpScriptManagerTests
{
    private const string SCRIPT_PATH = "TestScript.cs";

    [TestMethod]
    public void TestScriptLoading()
    {
        Mock<IIrcCommands> mockCommands = new();
        CSharpScriptManager csScriptMgr = new(mockCommands.Object);

        ICSharpScript? script = csScriptMgr.ExecuteScript<ICSharpScript>(SCRIPT_PATH);
        script?.Initialize();
        Assert.IsNotNull(script);

        ICSharpScript? script2 = csScriptMgr.ExecuteScript<ICSharpScript>(SCRIPT_PATH);
        script2?.Initialize();
        Assert.IsNotNull(script2);
    }
}