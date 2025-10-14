using spennyIRC.Scripting.Helpers;

namespace spennyIRC.Tests;

[TestClass]
public class DictionaryTests
{
    [TestMethod]
    public void TestDefine()
    {
        string dicks = WordLookupHelper.DefineAsync("fart").GetAwaiter().GetResult();
    }

    [TestMethod]
    public void TestDefine2()
    {
        List<UdDefinition> definitions = UdLookupHelper.UdLookupAsync("fart").GetAwaiter().GetResult();
    }
}