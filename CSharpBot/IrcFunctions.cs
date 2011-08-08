using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace CSharpBot
{
    public class IrcFunctions
    {
        // TODO: Implement more functions :-)

        CSharpBot CSharpBot;

        public IrcFunctions(CSharpBot bot)
        {
            this.CSharpBot = bot;
        }

        /// <summary>
        /// Checks if a hostmask fits to the owner's hostmask regex.
        /// </summary>
        /// <param name="inputmask">The input mask to check against the owner's hostmask</param>
        /// <returns>Boolean, which tells, if the inputmask fits the owner's hostmask regex</returns>
        public bool IsOwner(string inputmask)
        {
            Regex HostmaskRegex;
            HostmaskRegex = new Regex(CSharpBot.config.OwnerHostMask);
            return HostmaskRegex.Match(inputmask).Success;
        }

        /// <summary>
        /// Checks if a target is a channel
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsChannel(string target)
        {
            return target.StartsWith("#");
        }

        /// <summary>
        /// Logs a text.
        /// </summary>
        /// <param name="input">The text to log</param>
        public void Log(string input)
        {
            if (CSharpBot.config.EnableFileLogging)
            {
                string f = CSharpBot.config.Logfile;
                if (File.Exists(f))
                {
                    List<string> lines = new List<string>();
                    using (StreamReader r = new StreamReader(f))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                    using (StreamWriter w = new StreamWriter(f))
                    {
                        lines.Add(input);
                        foreach (string s in lines)
                        {
                            w.WriteLine(s);
                        }
                    }
                }
                else
                {
                    using (StreamWriter w = new StreamWriter(f))
                    {
                        w.WriteLine(input);
                    }
                }
                Console.WriteLine(input);
            }
            else
            {
                Console.WriteLine(input);
            }
        }

        /// <summary>
        /// Sends a text to a channel.
        /// </summary>
        /// <param name="channel">The target channel</param>
        /// <param name="text">The text</param>
        [Obsolete("You should rather use the PrivateMessage function.", false)] // to be replaced with "PrivateMessage"
        public void Say(string channel, string text)
        {
            if (channel.StartsWith("#"))
            {
                CSharpBot.writer.WriteLine("PRIVMSG " + channel + " :" + text);
            }
        }

        /// <summary>
        /// Tells the bot to join a channel.
        /// </summary>
        /// <param name="channel">The channel</param>
        public void Join(string channel) {
            CSharpBot.writer.WriteLine("JOIN " + channel);
        }

        /// <summary>
        /// Tells the server our username.
        /// </summary>
        /// <param name="username">The username. Mostly, the initial nickname.</param>
        /// <param name="realname">The realname.</param>
        /// <param name="invisible">Indicates, if you want to be invisible.</param>
        /// <param name="getwallops">Indicates, if you want to receive wallops</param>
        public void User(string username, string realname, bool invisible = false, bool getwallops = true)
        {
            string binary = "1" + (invisible ? "1" : "0") + (getwallops ? "1" : "0");
            CSharpBot.writer.WriteLine("USER " + username + " " + Convert.ToUInt16(binary, 2).ToString() + " * :" + realname);
        }
        
        /// <summary>
        /// Tells the server our password.
        /// </summary>
        /// <param name="password">The password</param>
        public void Pass(string password)
        {
            CSharpBot.writer.WriteLine("PASS " + password);
        }

        /// <summary>
        /// Sets or changes the nickname.
        /// </summary>
        /// <param name="nickname">The nickname</param>
        public void Nick(string nickname)
        {
            CSharpBot.writer.WriteLine("NICK " + nickname);
        }

        /// <summary>
        /// Sends a private message.
        /// </summary>
        /// <param name="target">The target, may be a nickname or a channel</param>
        /// <param name="text">The text to send</param>
        public void PrivateMessage(string target, string text)
        {
            CSharpBot.writer.WriteLine("PRIVMSG " + target + " :" + text);
        }

        /// <summary>
        /// Sends a notice.
        /// </summary>
        /// <param name="target">The target, may be a nickname or a channel</param>
        /// <param name="text">The text to send</param>
        public void Notice(string target, string text)
        {
            CSharpBot.writer.WriteLine("NOTICE " + target + " :" + text);
        }

        /// <summary>
        /// Sends a delayed notice.
        /// </summary>
        /// <param name="target">The target, may be a nickname or a channel</param>
        /// <param name="text">The text to send</param>
        /// <param name="delayMilliseconds">Tells, how many milliseconds the message should be delayed</param>
        public void DelayNotice(string target, string text, int delayMilliseconds = 500)
        {
            Thread.Sleep(delayMilliseconds);
            CSharpBot.writer.WriteLine("NOTICE " + target + " :" + text);
        }

        /// <summary>
        /// Sends RAW IRC.
        /// </summary>
        /// <param name="rawline">The RAW IRC to send.</param>
        public void Raw(string rawline)
        {
            CSharpBot.writer.WriteLine(rawline);
        }

        public void Action(string target, string text)
        {
            PrivateMessage(target, "\u0001" + "ACTION " + text + "\u0001");
        }

        /// <summary>
        /// Outputs help to an IRC user (should be used threaded).
        /// </summary>
        /// <param name="o">Set by Thread instance, the nickname</param>
        public void SendHelp(object o)
        {
            string[] param = (string[])o;
            string nick = param[0];
            string hostmask = param[1];
            string prefix = param[2];
            
            Notice(nick, "Bot commands:");
            Notice(nick, "Everything in <> is necessary and everything in [] are optional.");

            DelayNotice(nick, prefix + "help -- This command.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "mode <mode> -- Sets a mode the current channel.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "topic [topic] -- Tells the current topic OR sets the channel's topic to [topic]");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "config <list|edit> [<variable> <value>] -- Tells current config.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "join <chan> -- Joins the bot to a channel");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "part <chan> [reason] -- Parts the bot from a channel");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "kick <nick> [reason] -- Kicks <nick> from the current channel for [reason], or, if [reason] is not specified, kicks user with one of the kick lines in the kicks database.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "kicklines <add|clear|read|total> <kickmessage|(do nothing)|number|(do nothing)> -- Does various actions to the kicklines database.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "reset -- Clears the config and restarts the bot");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "restart -- Restarts the bot");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "clean -- Clears the config and kills the bot");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "die [quitmessage] -- Kills the bot, with optional [quitmessage]");
            DelayNotice(nick, prefix + "time [<+|-> <number>] -- Tells the time in GMT/UTC, with the offset you specify.");
            DelayNotice(nick, prefix + "uptime -- Tells the time, which the bot is running now without crash or shutdown.");
        }

        /// <summary>
        /// Sends raw data and logs it in the console.
        /// </summary>
        /// <param name="data">Raw IRC data</param>
        public void WriteData(string data)
        {
            CSharpBot.writer.WriteLine(data);
            if (CSharpBot.DebuggingEnabled == true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("SEND: " + data);
                Console.ResetColor();
            }
        }
    }
}
//From hereby on, we post quotes of funny IRC as a sort of source code easter egg.
/*
 * --------------------
 * <+Champion03> I cannot cause a complete network failure.
 * <+Champion03> That was an act of god.
 * * +Champion03 (webchat@HARHAR-jog.9sj.1nmndk.IP) Quit (Killed (Merbo (This was an act of GOD!)))
 * --------------------
 */