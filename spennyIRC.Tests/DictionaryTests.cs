using spennyIRC.Scripting.Helpers;
using spennyIRC.Scripting.Helpers.UrbanDictionary;

namespace spennyIRC.Tests;

[TestClass]
public class DictionaryTests
{
    [TestMethod]
    public void DictDotOrgLookup()
    {
        string dict = DictLookupHelper.DefineAsync("test").GetAwaiter().GetResult();
        Assert.IsFalse(string.IsNullOrWhiteSpace(dict));
    }

    [TestMethod]
    public void UrbanDictionaryLookup()
    {
        List<UdDefinition> definitions = UdLookupHelper.UdLookupAsync("test").GetAwaiter().GetResult();
        Assert.IsTrue(definitions.Count > 0);
    }
}