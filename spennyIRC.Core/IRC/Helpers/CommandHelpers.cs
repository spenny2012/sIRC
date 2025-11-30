namespace spennyIRC.Core.IRC.Helpers;

public static class CommandHelpers
{
    public static IrcCommandInfo ExtractCommand(this string command)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command, nameof(command));

        ReadOnlySpan<char> commandSpan = command.AsSpan();
        int spaceIndex = commandSpan.IndexOf(' ');

        if (spaceIndex == -1)
        {
            return new IrcCommandInfo(
                command,
                null);
        }

        string parsedCmd = commandSpan[..spaceIndex].ToString();
        string parameters = commandSpan[(spaceIndex + 1)..].Trim().ToString();

        return new IrcCommandInfo(
            parsedCmd,
            parameters);
    }

    public static IrcCommandParametersInfo ExtractCommandParameters(this string? parameters)
    {
        return new IrcCommandParametersInfo(parameters, parameters?.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static IrcCommandParametersInfo ExtractCommandParameters(this IrcCommandInfo commandInfo)
    {
        return ExtractCommandParameters(commandInfo.Parameters);
    }
}

public readonly record struct IrcCommandInfo(string Command, string? Parameters);

public readonly record struct IrcCommandParametersInfo(string? Parameters, string[]? LineParts);
//{
//    public bool HasParams() { return Parameters != null && LineParts != null && LineParts.Length > 0; }
//}