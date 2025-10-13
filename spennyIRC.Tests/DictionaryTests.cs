namespace spennyIRC.Tests;

using spennyIRC.Scripting.Helpers;

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
        string dicks = WordLookupHelper.UdLookupAsync("fart").GetAwaiter().GetResult();
    }
}