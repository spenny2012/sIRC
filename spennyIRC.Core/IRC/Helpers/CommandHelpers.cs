namespace spennyIRC.Core.IRC.Helpers;

public static class CommandHelpers
{
    public static ExtractedCommandInfo ExtractCommandAndParams(this string command)
    {
        int firstSpaceIndex = command.IndexOf(' ');

        if (firstSpaceIndex == -1)
            return new ExtractedCommandInfo(command[1..], null);

        string parsedCmd = command[1..firstSpaceIndex];
        ReadOnlySpan<char> paramSpan = command.AsSpan(firstSpaceIndex + 1);
        int start = 0;

        while (start < paramSpan.Length && char.IsWhiteSpace(paramSpan[start]))
            start++;

        int end = paramSpan.Length - 1;

        while (end >= start && char.IsWhiteSpace(paramSpan[end]))
            end--;

        string? cmdParameters = null;

        if (start <= end)
            cmdParameters = paramSpan.Slice(start, end - start + 1).ToString();

        return new ExtractedCommandInfo(parsedCmd, cmdParameters);
    }
}

public readonly record struct ExtractedCommandInfo(string ParsedCmd, string? CmdParameters);