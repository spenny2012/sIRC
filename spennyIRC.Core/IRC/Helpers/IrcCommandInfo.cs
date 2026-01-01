namespace spennyIRC.Core.IRC.Helpers;

public readonly record struct IrcCommandInfo(string Command, string? Parameters)
{
    public bool HasParameters => !string.IsNullOrWhiteSpace(Parameters);
}
