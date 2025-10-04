using spennyIRC.Core.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spennyIRC.ViewModels.Messages.Window
{
    public class NickChangedMessage(IIrcSession session) : MessageBase(session)
    {
        public string Nick { get; set; }
        public string NewNick { get; set; }
    }
}