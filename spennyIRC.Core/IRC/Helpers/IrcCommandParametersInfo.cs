namespace spennyIRC.Core.IRC.Helpers;

public readonly record struct IrcCommandParametersInfo(string? Parameters, string[]? ParemeterParts, bool HasParameters)
{
    public T? GetParam<T>(int arg)
    {
        if (ParemeterParts == null ||
            arg < 0 ||
            arg > ParemeterParts.Length - 1)
            return default;

        string raw = ParemeterParts[arg];

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
