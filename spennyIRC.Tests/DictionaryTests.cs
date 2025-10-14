using spennyIRC.Scripting.Helpers;
using spennyIRC.Scripting.Helpers.UrbanDictionary;

namespace spennyIRC.Tests;

[TestClass]
public class DictionaryTests
{
    [TestMethod]
    public void TestDefine()
    {
        string dicks = DictLookupHelper.DefineAsync("test").GetAwaiter().GetResult();
    }

    [TestMethod]
    public void TestDefine2()
    {
        List<UdDefinition> definitions = UdLookupHelper.UdLookupAsync("tttttt").GetAwaiter().GetResult();
    }
}