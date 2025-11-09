using spennyIRC.Core.IRC.Constants;
using spennyIRC.Core.IRC.Helpers;

namespace spennyIRC.Core.IRC;

public static class IrcReceivedContextFactory
{
    private const string DISCONNECT = "DISCONNECT";

    public static IIrcReceivedContext CreateDisconnect(IIrcSession session, string line)
    {
        IrcReceivedContext ctx = new()
        {
            Session = session,
            Line = line,
            LineParts = { },
            Event = DISCONNECT,
            Recipient = string.Empty
        };
        return ctx;
    }

    public static IIrcReceivedContext Create(IIrcSession session, string line, string[] lineParts)
    {
        // create basic context
        IrcReceivedContext ctx = new()
        {
            Session = session,
            Line = line,
            LineParts = lineParts,
            Event = lineParts.Length > 1 ? lineParts[1] : "UNKNOWN",
            Recipient = lineParts.Length > 2 ? lineParts[2] : string.Empty
        };

        // check for disconnect
        if (string.Equals(lineParts[0], "ERROR") && string.Equals(lineParts[1], ":Closing Link:"))
        {
            ctx.Event = DISCONNECT; // TODO: parse this out better
            return ctx;
        }

        string trailing = ctx.Trailing = line.ExtractTrailing(2);

        // check if event is int
        if (int.TryParse(ctx.Event, out _))
            return ctx;

        ReadOnlySpan<char> firstPart = lineParts[0].AsSpan();
        int index = firstPart.IndexOf('!');
        ctx.Nick = index == -1 ? firstPart[1..].ToString() : firstPart[1..index].ToString();

        // if is ctcp
        if (ctx.Event == "PRIVMSG" && trailing[..1][0] == ProtocolCharacterConstants.CTCP_CHAR)
        {
            string ctcpType = trailing[1..];
            if (ctcpType[^1..][0] == ProtocolCharacterConstants.CTCP_CHAR)
                ctcpType = ctcpType[..^1];
            ctx.Event = ctcpType;
        }

        return ctx;
    }
}