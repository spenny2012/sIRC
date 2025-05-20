namespace spennyIRC.Core.IRC;

public readonly record struct IrcExtractedUserInfo()
{
    public string Nick { get; init; } = string.Empty;
    public string? Ident { get; init; }
    public string? Domain { get; init; }
    public IrcChannelAccessType[] Access { get; init; } = [];
}
