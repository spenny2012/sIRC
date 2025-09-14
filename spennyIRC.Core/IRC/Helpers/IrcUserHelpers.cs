using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spennyIRC.Core.IRC.Helpers;

public static class IrcUserHelpers
{
    public static string? GetHost(this IIrcUser user)
    {
        if (string.IsNullOrEmpty(user.Ident) || string.IsNullOrEmpty(user.Domain))
            return null;
        return $"{user.Ident}@{user.Domain}";
    }
}
