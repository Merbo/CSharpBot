using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    /// <summary>
    /// Represents a numeric reply
    /// </summary>
    public class IrcNumericReplyLine
    {
        string source;
        IrcReplyCode replycode;
        string target;
        string message;

        public IrcNumericReplyLine(string inputline)
        {
            string[] rawsplit = inputline.Split(' ');
            source = rawsplit[0];
            replycode = (IrcReplyCode)int.Parse(rawsplit[1]);
            target = rawsplit[2];
            if (rawsplit.Length > 3)
            {
                message = string.Join(" ", rawsplit.Skip(3).ToArray()).TrimStart(':');
            }
            else
            {
                message = ""; // No message
            }
        }

        public string Source
        {
            get { return source; }
        }

        public IrcReplyCode ReplyCode
        {
            get { return replycode; }
        }

        public string Target
        {
            get { return target; }
        }

        public string Message
        {
            get { return message; }
        }
    }
}
