using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    /// <summary>
    /// Represents an IRC privmsg/notice line.
    /// </summary>
    public class IrcMessageLine
    {
        string[] cmd;
        string nick; // (*)!*@*
        string ident; //*!(*)@*
        string host; // *!*@(*)
        string hostmask;
        string target;
        string message;
        XmlConfiguration config;

        public IrcMessageType MessageType
        {
            get {
                return
                cmd[1] == "PRIVMSG" && message.StartsWith("\x01") && message.EndsWith("\x01") ? IrcMessageType.CtcpRequest
                : (cmd[1] == "NOTICE" && message.StartsWith("\x01") && message.EndsWith("\x01") ? IrcMessageType.CtcpReply
                : (cmd[1] == "PRIVMSG" ? IrcMessageType.PrivateMessage
                : (cmd[1] == "NOTICE" ? IrcMessageType.Notice
                : IrcMessageType.Unknown)))
                ;
            }
        }

        public string SourceNickname
        {
            get { return nick; }
        }

        public string SourceHostmask
        {
            get { return hostmask; }
        }

        public string SourceUsername // or SourceIdent, but officially (RFC) it's User.
        {
            get { return ident; }
        }

        public string SourceHost
        {
            get { return host; }
        }

        public string Target // may also be CSharpBot itself
        {
            get { return target; }
        }

        public string Message
        {
            get { return message.Trim('\x01', '\n', '\r', '\t', ' ', '\0'); }
        }

        public bool IsBotCommand
        {
            get { return message.StartsWith(config.Prefix); }
        }

        public string BotCommandName
        {
            get { return IsBotCommand ? message.Split(' ')[0].Substring(config.Prefix.Length) : null; }
        }

        public string[] BotCommandParams
        {
            get {
                if (!IsBotCommand)
                    return null;
                string[] spl1 = message.Split('"');
                List<string> args = new List<string>();
                for (int i = 0; i < spl1.Length; i++)
                {
                    if (i / 2 == Math.Round((decimal)i / 2, 0))
                    {
                        string[] spl2 = spl1[i].Split(' ');
                        args.AddRange(spl2); // argument: ... ... ... are multiple args
                    }
                    else
                        args.Add(spl1[i]); // argument: "... ..." is 1 arg
                }
                return args.ToArray();
            }
        }

        public IrcMessageLine(string raw, XmlConfiguration xml)
        {
            cmd = raw.Split(' ');
            hostmask = cmd[0].Substring(1);
            string messagetype = cmd[1];
            target = cmd[2];
            message = string.Join(" ", cmd.Skip(3).ToArray()).Substring(1);

            string[] prenick = hostmask.Split(new char[] { '!', '@' });
            nick = prenick[0];
            if (prenick.Length > 1) this.ident = prenick[1];
            if (prenick.Length > 2) this.host = prenick[2];

            message = string.Join(" ", cmd.Skip(3).ToArray()).Substring(1);

            config = xml;
        }
    }

    public enum IrcMessageType
    {
        Notice = 1,
        PrivateMessage = 0,
        Unknown = 0xffff,
        CtcpReply = 0xf001,
        CtcpRequest = 0xf000
    }
}
