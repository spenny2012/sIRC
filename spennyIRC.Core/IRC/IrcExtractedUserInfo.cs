using System.Text;

namespace spennyIRC.Core.IRC;

public readonly record struct IrcExtractedUserInfo()
{
    public required string Nick { get; init; }
    public string? Ident { get; init; }
    public string? Domain { get; init; }
    public IrcChannelAccessType[] Access { get; init; } = [];
}
//public string FullHost
//{
//    get
//    {
//        var sb = $"{Nick}!";
//        if (!string.IsNullOrWhiteSpace(Ident))
//        {
//            sb += Ident;
//        }
//        else
//        {
//            sb += '*';
//        }

//        sb += '@';

//        if (!string.IsNullOrWhiteSpace(Domain))
//        {
//            sb += Domain;
//        }

//        return sb;
//    }
//}