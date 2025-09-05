namespace spennyIRC.Core.IRC;

public readonly record struct IrcExtractedUserInfo()
{
    public required string Nick { get; init; }
    public string? Ident { get; init; }
    public string? Domain { get; init; }
    public IrcChannelAccessType[] Access { get; init; } = [];
}
