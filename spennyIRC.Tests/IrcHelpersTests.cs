using spennyIRC.Core.IRC;
using spennyIRC.Core.IRC.Helpers;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace spennyIRC.Tests;

[TestClass]
public sealed class IrcHelpersTests
{
    [TestMethod]
    public void Test353Parse()
    {
        string exampleString = "@+test!~test@214.59.220.43 e!~e@36CBC3F1.896F4908.4BE6EE2D.IP %TestGuy!~testguy@1a3b:8df6:7ecd:2aba:e662:98e1:325b:92f8";
        string[] toLoop = exampleString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (string item in toLoop)
        {
            IrcExtractedUserInfo info = item.ExtractUserInfoFrom353();
            Debug.WriteLine(info.Nick);
            Debug.WriteLine(info.Ident);
            Debug.WriteLine(info.Domain);
            Debug.WriteLine($"Access: {info.Access.Length}");
        }
    }

    [TestMethod]
    public void Test352Parse()
    {
        string unreal = @":war.test.org 352 Me #test ~unknown 86D16924.E73DE4A0.44413713.IP * terminatrix Hs+ :0 unknown
:war.test.org 352 Me #test ~corker 994C0BFC.BD9F3164.AA7CA170.IP * corker_ H+ :0 corker
:war.test.org 352 Me #test ~weev 368B554D.BC031AB8.35A3430C.IP * TEST Hs@+ :0 weev
:war.test.org 352 Me #test ~elDoberma E82DFF35.FF5108BE.3D408167.IP * elDoberman Hs+ :0 elDoberman
:war.test.org 352 Me #test vorteckz D7DE6911.D7AB44E4.3964A1F7.IP * vorteckz2 Hs+ :0 vorteckz
:war.test.org 352 Me #test ~sad 4770F27C.8324A9D7.D453684D.IP * sad Hrs+ :0 sad
:war.test.org 352 Me #test moony 5BFC9E5B.FAAA9194.15265305.IP * moony Hs+ :0 moony
:war.test.org 352 Me #test ~shhh prettylittle.ninja * pankakes Hrs+ :0 pancakes
:war.test.org 352 Me #test ~chzz 7A0ACD13.67901C6D.7876F6F8.IP * chzz Hs+ :0 chzz
:war.test.org 352 Me #test ~sid11594 31247A87.BB0D0323.FD0AB6F1.IP * octo Hs+ :0 cockmangler
";
        string[] toLoop = unreal.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        List<IrcExtractedUserInfo> entries = [];
        foreach (string line in toLoop)
        {
            string[] lineParts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            IrcExtractedUserInfo info = lineParts.ExtractUserInfoFrom352();
            entries.Add(info);
            Debug.WriteLine($"Name: {info.Nick}");
            Debug.WriteLine($"Ident: {info.Ident}");
            Debug.WriteLine($"Domain: {info.Domain}");
            string access = string.Join(',', info.Access.Select(y => y.ToString()));
            Debug.WriteLine($"Access: {access}");
            Debug.WriteLine($"--------------------------");
        }

        Assert.IsTrue(entries.Count == 10);
        IrcExtractedUserInfo eighthEntry = entries[8];
        Assert.IsTrue(eighthEntry.Nick == "chzz");
        Assert.IsTrue(eighthEntry.Ident == "~chzz");
        Assert.IsTrue(eighthEntry.Domain == "7A0ACD13.67901C6D.7876F6F8.IP");
    }

    [TestMethod]
    public void TestExtractUserModes()
    {
        string exampleString = @":ASDASDASD!~TEST@5F89E267.BFF0CD52.EB5654DF.IP MODE #test -h YourNick
:ASDASDASD!~TEST@5F89E267.BFF0CD52.EB5654DF.IP MODE #test +h-ov YourNick YourNick YourNick
:ASDASDASD!~TEST@5F89E267.BFF0CD52.EB5654DF.IP MODE #test +h-ovv YourNick YourNick YourNick ASDASDASD";
        string[] lines = exampleString.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            Dictionary<string, List<(bool isAdding, IrcChannelAccessType type)>> userUpdates = line.Split(' ').ExtractChannelUserModes();
            foreach (KeyValuePair<string, List<(bool isAdding, IrcChannelAccessType type)>> user in userUpdates)
            {
                foreach ((bool isAdding, IrcChannelAccessType type) in user.Value)
                {
                    Debug.WriteLine($"{(isAdding ? "ADD" : "REMOVE")} {type} {(isAdding ? "TO" : "FROM")} {user.Key}");
                }
            }
        }
    }

    [TestMethod]
    public void TestExtractTrailingFromPrivmsg()
    {
        string testData = @":YourNick!~Hello@1a3b:8df6:7ecd:2aba:e662:98e1:325b:92f8 PRIVMSG #test :test ':'
:YourNick2!~Hello2@45.13.235.55 PRIVMSG #test :test ':'
:YourNick!~Hello@1a3b:8df6:7ecd:2aba:e662:98e1:325b:92f8 PRIVMSG #test :test ':'
:YourNick2!~Hello2@45.13.255.55 PRIVMSG #test :test ':'
:Testbot!~Bot@90EE3181.D23D4749.DD274DCD.IP QUIT :Ping timeout: 240 seconds
:YourNick!~Hello@1a3b:8df6:7ecd:2aba:e662:98e1:325b:92f8 QUIT :Ping timeout: 240 seconds
:YourNick!~Hello@1a3b:8df6:7ecd:2aba:e662:98e1:325b:92f8 JOIN :#test
:YourNick!~Hello@1a3b:8df6:7ecd:2aba:e662:98e1:325b:92f8 PRIVMSG #test :test ':'
:irc.psychz.net 003 YourNick :This server was created Sun May 23 2021 at 19:07:21 UTC
";
        string[] testLines = testData.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in testLines)
        {
            string trailing = line.ExtractTrailing(2);
        }
    }

    [TestMethod]
    public void Test005Parse()
    {
        string testData = @":irc.atw-inter.net 005 YourNick RFC2812 PREFIX=(ov)@+ CHANTYPES=#&!+ MODES=3 CHANLIMIT=#&!+:42 NICKLEN=15 TOPICLEN=255 KICKLEN=255 MAXLIST=beIR:64 CHANNELLEN=50 IDCHAN=!:5 CHANMODES=beIR,k,l,imnpstaqrzZ :are supported by this server
:irc.atw-inter.net 005 YourNick PENALTY FNC EXCEPTS=e INVEX=I CASEMAPPING=ascii NETWORK=IRCnet :are supported by this server";
        string pattern = @"(\S+)=([^\s]+)";
        Regex regex = new(pattern);

        string[] testDataParts = testData.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in testDataParts)
        {
            MatchCollection matches = regex.Matches(line);
            foreach (Match match in matches)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                Debug.WriteLine($"Key: {key}, Value: {value}");
            }
        }
    }
}
