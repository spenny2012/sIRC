using Microsoft.VisualStudio.TestTools.UnitTesting;
using spennyIRC.Core.IRC.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spennyIRC.Tests.CommandTests;

[TestClass]
public class ExtractCommandTests
{
    [TestMethod]
    public void ExtractCommandTest()
    {
        const string MY_COMMAND = "/test  10    HELLO WORLD!";
        var cmd = MY_COMMAND.AsSpan(1).ToString().ExtractCommand();
        var prms = cmd.Parameters?.ExtractCommandParameters();
    }
}
