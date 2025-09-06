using spennyIRC.Core.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spennyIRC.ViewModels.Messages
{
    public class ClearWindowMessage(IIrcSession session) : MessageBase(session)
    {
    }
}
