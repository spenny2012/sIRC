namespace spennyIRC.Core.IRC.Helpers;

public static class CommandHelpers
{
    public static IrcCommandInfo CreateCommandInfo(this string command) => IrcCommandInfo.Create(command);

    public static IrcCommandParametersInfo CreateCommandParameters(this string? parameters)
    {
        return new IrcCommandParametersInfo(parameters, parameters?.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

public readonly record struct IrcCommandInfo(string Command, string? Parameters)
{
    public bool HasParameters => !string.IsNullOrWhiteSpace(Parameters);

    public IrcCommandParametersInfo CreateParameters() => Parameters.CreateCommandParameters();

    public static IrcCommandInfo Create(string command)
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
}

public readonly record struct IrcCommandParametersInfo(string? Parameters, string[]? LineParts)
{
    public bool HasParameters => !string.IsNullOrWhiteSpace(Parameters) && LineParts?.Length > 0;

    public T? GetParam<T>(int arg)
    {
        if (LineParts == null ||
            arg < 0 ||
            arg > LineParts.Length - 1)
            return default;

        string raw = LineParts[arg];

        if (string.IsNullOrWhiteSpace(raw))
            return default;

        try
        {
            Type mainType = typeof(T);
            Type u = Nullable.GetUnderlyingType(mainType) ?? mainType;
            return (T?) Convert.ChangeType(raw, u);
        }
        catch
        {
            return default;
        }
    }

    public string? GetParams(int startArg)
    {
        return Parameters?.GetTokenFrom(startArg);
    }
}
//public bool TryExtractCommandParameters(out IrcCommandParametersInfo? commandParamatersInfo)
//{
//    commandParamatersInfo = null;

//    if (string.IsNullOrWhiteSpace(Parameters)) return false;

//    var parameters = Parameters.ExtractCommandParameters();
//    if (!parameters.HasParameters) return false;

//    commandParamatersInfo = parameters; 
//    return true;
//}