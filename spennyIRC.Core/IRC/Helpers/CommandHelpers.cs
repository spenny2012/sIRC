namespace spennyIRC.Core.IRC.Helpers;

public static class CommandHelpers
{
    public static IrcCommandInfo CreateCommand(this string command)
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

        string cmd = commandSpan[..spaceIndex].ToString();
        string parameters = commandSpan[(spaceIndex + 1)..].Trim().ToString();

        return new IrcCommandInfo(
            cmd,
            parameters);
    }

    public static IrcCommandParametersInfo CreateParameters(this string? parameters)
    {
        string[]? lineParts = parameters?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bool hasParameters = !string.IsNullOrWhiteSpace(parameters) && lineParts?.Length > 0;

        return new IrcCommandParametersInfo(parameters, lineParts, hasParameters);
    }

    public static IrcCommandParametersInfo CreateParameters(this IrcCommandInfo command)
    {
        return command.Parameters.CreateParameters();
    }
}
