using System.Text.RegularExpressions;

namespace spennyIRC.Core.IRC.Helpers;

public static class IrcProtocolHelpers
{
    //public static string ExtractFullHost(this string line)
    //{
    //    return line[(line.IndexOf('!') + 1)..];
    //}
    public static ReadOnlySpan<char> ExtractFullHost(this string line)
    {
        ReadOnlySpan<char> span = line.AsSpan();
        int index = span.IndexOf('!');
        return index == -1 ? [] : span[(index + 1)..];
    }

    public static string GetTokenFrom(this string line, int tokenIndex, char delimiter = ' ')
    {
        ReadOnlySpan<char> span = line.AsSpan();
        int currentIndex = 0;
        int partCount = 0;

        while (partCount < tokenIndex)
        {
            int spaceIndex = span[currentIndex..].IndexOf(delimiter);
            if (spaceIndex == -1)
            {
                return string.Empty;
            }
            currentIndex += spaceIndex + 1;
            partCount++;
        }

        return span[currentIndex..].ToString();
    }

    public static string GetTokenFrom(this string line, int startTokenIndex, int endTokenIndex, char delimiter = ' ')
    {
        if (startTokenIndex < 0 || endTokenIndex < startTokenIndex)
        {
            return string.Empty;
        }

        ReadOnlySpan<char> span = line.AsSpan();
        int currentIndex = 0;
        int tokenCount = 0;
        int tokenStartPos = -1;
        int tokenEndPos = -1;

        // Find the start position of the startTokenIndex
        while (tokenCount < startTokenIndex && currentIndex < span.Length)
        {
            int delimiterIndex = span[currentIndex..].IndexOf(delimiter);
            if (delimiterIndex == -1)
            {
                return string.Empty; // Not enough tokens
            }
            currentIndex += delimiterIndex + 1;
            tokenCount++;
        }

        if (currentIndex >= span.Length)
        {
            return string.Empty; // Start token doesn't exist
        }

        tokenStartPos = currentIndex;

        // Find the end position after the endTokenIndex
        while (tokenCount <= endTokenIndex && currentIndex < span.Length)
        {
            int delimiterIndex = span[currentIndex..].IndexOf(delimiter);
            if (delimiterIndex == -1)
            {
                // We've reached the end of the string
                tokenEndPos = span.Length;
                break;
            }
            currentIndex += delimiterIndex;

            if (tokenCount == endTokenIndex)
            {
                tokenEndPos = currentIndex;
                break;
            }

            currentIndex++; // Move past the delimiter
            tokenCount++;
        }

        if (tokenEndPos == -1)
        {
            return string.Empty; // End token doesn't exist
        }

        return span[tokenStartPos..tokenEndPos].ToString();
    }

    public static string ExtractTrailing(this string line, int startIndex) // TODO: make this less weird
    {
        ReadOnlySpan<char> span = line.AsSpan();
        int currentIndex = 0;
        int partCount = 0;

        while (partCount < startIndex)
        {
            int spaceIndex = span[currentIndex..].IndexOf(' ');
            if (spaceIndex == -1)
            {
                return string.Empty;
            }
            currentIndex += spaceIndex + 1;
            partCount++;
        }

        ReadOnlySpan<char> remaining = span[currentIndex..];
        int colonIndex = remaining.IndexOf(':');
        if (colonIndex == -1)
        {
            return remaining.ToString();
        }

        return remaining[(colonIndex + 1)..].ToString();
    }

    public static IrcExtractedUserInfo ExtractBasicUserInfo(this string fullNick)
    {
        ReadOnlySpan<char> fullNickSpan = fullNick.AsSpan();
        int nickEnd = fullNickSpan.IndexOf('!');
        if (nickEnd < 0)
        {
            throw new FormatException("Invalid format: '!' not found.");
        }

        int identEnd = fullNickSpan[(nickEnd + 1)..].IndexOf('@');
        if (identEnd < 0)
        {
            throw new FormatException("Invalid format: '@' not found.");
        }
        identEnd += nickEnd + 1;

        return new IrcExtractedUserInfo
        {
            Nick = fullNickSpan[..nickEnd].ToString(),
            Ident = fullNickSpan[(nickEnd + 1)..identEnd].ToString(),
            Domain = fullNickSpan[(identEnd + 1)..].ToString()
        };
    }

    public static IrcChannelAccessType ModeLetterToAccessType(this char c)
    {
        return c switch
        {
            'o' => IrcChannelAccessType.OP,
            'h' => IrcChannelAccessType.HOP,
            'v' => IrcChannelAccessType.VOICE,
            'b' => IrcChannelAccessType.BAN,
            'e' => IrcChannelAccessType.EXEMPT,
            'q' => IrcChannelAccessType.FOUNDER,
            'a' => IrcChannelAccessType.SOP,
            _ => IrcChannelAccessType.NONE,
        };
    }

    private static IrcChannelAccessType AccessSymbolToAccessType(this char c)
    {
        return c switch
        {
            '@' => IrcChannelAccessType.OP,
            '%' => IrcChannelAccessType.HOP,
            '+' => IrcChannelAccessType.VOICE,
            '~' => IrcChannelAccessType.FOUNDER,
            '&' => IrcChannelAccessType.SOP,
            _ => IrcChannelAccessType.NONE,
        };
    }

    /// <summary>
    /// Extracts channel user mode changes from IRC message parts.
    /// </summary>
    /// <param name="messageParts">The IRC message split into parts, where index 3 is the mode string and 4 onward are user names.</param>
    /// <returns>A dictionary mapping users to their mode changes, with each change as a tuple of (isAdding, accessType).</returns>
    /// <exception cref="ArgumentException">Thrown if messageParts has fewer than 4 elements.</exception>
    public static Dictionary<string, List<(bool isAdding, IrcChannelAccessType type)>> ExtractChannelUserModes(this string[] messageParts)
    {
        string modeString = messageParts[3];
        string[] affectedUsers = messageParts[4..];
        Dictionary<string, List<(bool isAdding, IrcChannelAccessType type)>> userModeChanges = [];

        bool isAdding = false;
        int userIndex = 0;

        for (int i = 0; i < modeString.Length; i++)
        {
            char currentChar = modeString[i];
            if (currentChar == '+')
            {
                isAdding = true;
            }
            else if (currentChar == '-')
            {
                isAdding = false;
            }
            else if (char.IsLetter(currentChar) && userIndex < affectedUsers.Length)
            {
                string user = affectedUsers[userIndex];
                IrcChannelAccessType accessType = currentChar.ModeLetterToAccessType();

                if (accessType != IrcChannelAccessType.NONE)
                {
                    if (userModeChanges.TryGetValue(user, out List<(bool isAdding, IrcChannelAccessType type)>? changes))
                    {
                        changes.Add((isAdding, accessType));
                    }
                    else
                    {
                        userModeChanges[user] = [(isAdding, accessType)];
                    }
                }
                userIndex++;
            }
        }

        return userModeChanges;
    }

    public static (string nick, IrcChannelAccessType[] access) ExtractAccessFromNick(this string nick) // TODO: maybe redo this entirely.. this is for 353 and 353 only seems to show 1 channel access mode on IRCnet
    {
        Span<IrcChannelAccessType> accessSpan = stackalloc IrcChannelAccessType[5];
        int accessCount = 0;
        ReadOnlySpan<char> nickSpan = nick.AsSpan();

        int maxLength = nickSpan.Length < 5 ? nickSpan.Length : 5;
        for (int i = 0; i < maxLength; i++)
        {
            char currentChar = nickSpan[i];
            IrcChannelAccessType accessType = currentChar.AccessSymbolToAccessType();
            if (accessType != IrcChannelAccessType.NONE)
            {
                accessSpan[accessCount++] = accessType;
            }
        }

        string resultNick = accessCount > 0 ? nickSpan[accessCount..].ToString() : nick;
        IrcChannelAccessType[] resultAccess = accessSpan[..accessCount].ToArray();

        return (resultNick, resultAccess);
    }

    public static (string nick, IrcChannelAccessType[] access) ExtractAccessFromNick(this ReadOnlySpan<char> nickSpan)
    {
        // Use stackalloc for temporary storage of access types
        Span<IrcChannelAccessType> accessSpan = stackalloc IrcChannelAccessType[5];
        int accessCount = 0;

        // Iterate through the nick to extract access prefixes
        int maxLength = nickSpan.Length < 5 ? nickSpan.Length : 5;
        for (int i = 0; i < maxLength; i++)
        {
            char currentChar = nickSpan[i];
            IrcChannelAccessType accessType = currentChar.AccessSymbolToAccessType();
            if (accessType != IrcChannelAccessType.NONE)
            {
                accessSpan[accessCount++] = accessType;
            }
        }

        // Extract the nick without access prefixes
        string resultNick = accessCount > 0 ? nickSpan[accessCount..].ToString() : nickSpan.ToString();
        IrcChannelAccessType[] resultAccess = accessSpan[..accessCount].ToArray();
        return (resultNick, resultAccess);
    }

    /// <summary>
    /// This function is to be used for numeric 352
    /// </summary>
    ///
    public static IrcChannelAccessType[] ExtractAccessTypeFrom352(this string modes)
    {
        Span<IrcChannelAccessType> accessSpan = stackalloc IrcChannelAccessType[5]; // Max 5 access types
        int accessCount = 0;
        ReadOnlySpan<char> modesSpan = modes.AsSpan();

        for (int i = 0; i < modesSpan.Length; i++)
        {
            char mode = modesSpan[i];
            if (char.IsLetter(mode)) continue;

            IrcChannelAccessType accessType = mode.AccessSymbolToAccessType();
            if (accessType != IrcChannelAccessType.NONE && accessCount < 5)
                accessSpan[accessCount++] = accessType;
        }

        return accessSpan[..accessCount].ToArray();
    }

    public static IrcExtractedUserInfo ExtractUserInfoFrom353(this string fullHostString)
    {
        ReadOnlySpan<char> fullHostSpan = fullHostString.AsSpan();

        int nickEnd = fullHostSpan.IndexOf('!');
        if (nickEnd == -1)
        {
            (string nick, IrcChannelAccessType[] access) = ExtractAccessFromNick(fullHostSpan);
            return new IrcExtractedUserInfo
            {
                Nick = nick.ToString(),
                Access = access
            };
        }
        else
        {
            int identEnd = fullHostSpan[(nickEnd + 1)..].IndexOf('@');
            if (identEnd == -1)
            {
                throw new FormatException("Invalid format: '@' not found.");
            }
            identEnd += nickEnd + 1;

            ReadOnlySpan<char> nickSpan = fullHostSpan[..nickEnd];
            ReadOnlySpan<char> identSpan = fullHostSpan[(nickEnd + 1)..identEnd];
            ReadOnlySpan<char> domainSpan = fullHostSpan[(identEnd + 1)..];

            (string nick, IrcChannelAccessType[] access) = ExtractAccessFromNick(nickSpan);

            return new IrcExtractedUserInfo
            {
                Nick = nick.ToString(),
                Ident = identSpan.ToString(),
                Domain = domainSpan.ToString(),
                Access = access
            };
        }
    }

    public static IrcExtractedUserInfo ExtractUserInfoFrom352(this string[] lineParts)
    {
        string nick = lineParts[7];
        string ident = lineParts[4];
        string domain = lineParts[5];
        string userAccessModes = lineParts[8];
        IrcChannelAccessType[] access = userAccessModes.ExtractAccessTypeFrom352();
        return new IrcExtractedUserInfo
        {
            Nick = nick,
            Ident = ident,
            Domain = domain,
            Access = access
        };
    }

    public static Dictionary<string, string> ExtractNetworkSettingsFrom005(this string line)
    {
        Dictionary<string, string> dictionary = [];
        Regex regex = new(@"(\S+)=([^\s]+)");

        MatchCollection matches = regex.Matches(line);
        foreach (Match match in matches)
        {
            string key = match.Groups[1].Value;
            string value = match.Groups[2].Value;
            dictionary[key] = value;
        }
        return dictionary;
    }
}