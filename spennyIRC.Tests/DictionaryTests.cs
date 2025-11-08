using spennyIRC.Scripting.Helpers;
using spennyIRC.Scripting.Helpers.UrbanDictionary;

namespace spennyIRC.Tests;

[TestClass]
public class DictionaryTests
{
    [TestMethod]
    public void DictDotOrgLookup()
    {
        string dicks = DictLookupHelper.DefineAsync("test").GetAwaiter().GetResult();
        // TODO: add tests
    }

    [TestMethod]
    public void UrbanDictionaryLookup()
    {
        List<UdDefinition> definitions = UdLookupHelper.UdLookupAsync("tttttt").GetAwaiter().GetResult();
        Assert.IsTrue(definitions.Count > 0);
    }
}