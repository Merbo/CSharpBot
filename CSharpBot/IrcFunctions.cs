using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Reflection;
namespace CSharpBot
{
    public class IrcFunctions
    {
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
        /// Checks if a target is a channel.
        /// </summary>
        /// <param name="target">Target</param>
        /// <returns>If target is a channel</returns>
        public bool IsChannel(string target)
        {
            return target.StartsWith("#");
        }

        /// <summary>
        /// Lets the bot leave a channel.
        /// </summary>
        /// <param name="channel">The channel to leave</param>
        public void Part(string channel, string reason = "Just leaving")
        {
            WriteData("PART " + channel + " :" + reason);
        }

        /// <summary>
        /// Sets the topic.
        /// </summary>
        /// <param name="channel">Target channel</param>
        /// <param name="text">Text to set as topic</param>
        public void Topic(string channel, string text)
        {
            Console.WriteLine("TOPIC " + channel + " :" + text);
        }

        /// <summary>
        /// Sends QUIT to the server and disconnects.
        /// </summary>
        /// <param name="reason">Optional reason for QUIT</param>
        public void Quit(string reason = "Bye!")
        {
            try
            {
                WriteData("QUIT :" + reason);
            }
            catch (IOException) // Disconnected from server early
            {
                Console.WriteLine("Disconnected.");
            }
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
                WriteData("PRIVMSG " + channel + " :" + text);
            }
        }

        /// <summary>
        /// Tells the bot to join a channel.
        /// </summary>
        /// <param name="channel">The channel</param>
        public void Join(string channel)
        {
            CSharpBot.writer.WriteLine("JOIN " + channel);
        }

        /// <summary>
        /// Lets the bot rejoin a channel
        /// </summary>
        /// <param name="channel">The channel</param>
        public void Cycle(string channel)
        {
            CSharpBot.writer.WriteLine("PART " + channel + " :Cycling");
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
            WriteData("PASS " + password);
        }

        /// <summary>
        /// Sets or changes the nickname.
        /// </summary>
        /// <param name="nickname">The nickname</param>
        public void Nick(string nickname)
        {
            WriteData("NICK " + nickname);
        }

        /// <summary>
        /// Sends a private message.
        /// </summary>
        /// <param name="target">The target, may be a nickname or a channel</param>
        /// <param name="text">The text to send</param>
        public void PrivateMessage(string target, string text)
        {
            try
            {
                WriteData("PRIVMSG " + target + " :" + text);
            }
            catch (IOException x)
            {
                Console.Beep();
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write(@"THE CONNECTION TO THE SERVER WAS LOST!
The connection to the server was lost.
The guru meditation is:

" + x.Message);

                Console.Title = "ERROR";
                Console.ReadKey();
                CSharpBot.Shutdown("ERR", false);
            }
        }

        /// <summary>
        /// Sends a notice.
        /// </summary>
        /// <param name="target">The target, may be a nickname or a channel</param>
        /// <param name="text">The text to send</param>
        public void Notice(string target, string text)
        {
            WriteData("NOTICE " + target + " :" + text);
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
            WriteData("NOTICE " + target + " :" + text);
        }

        /// <summary>
        /// Sends a raw IRC line.
        /// </summary>
        /// <param name="rawline">The raw IRC line to send.</param>
        public void Raw(string rawline)
        {
            WriteData(rawline);
        }

        /// <summary>
        /// Sends an ACTION to a channel
        /// </summary>
        /// <param name="target">Target channel</param>
        /// <param name="text">Text to send</param>
        public void Action(string target, string text)
        {
            PrivateMessage(target, "\u0001" + "ACTION " + text + "\u0001");
        }

        /// <summary>
        /// Sends a MODE command to server.
        /// </summary>
        /// <param name="channel">The channel to set the mode in</param>
        /// <param name="command">The mode command (as in +o newop or +v newvoiceduser)</param>
        public void Mode(string channel, string command)
        {
            Raw("MODE " + channel + " " + command);
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
            Botop check = new Botop();

            Notice(nick, "Bot commands:");
            Notice(nick, "Everything in <> is necessary and everything in [] are optional.");

            DelayNotice(nick, prefix + "help -- This command.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "mode <mode> -- Sets a mode the current channel.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "topic [topic] -- Tells the current topic OR sets the channel's topic to [topic]");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "config <list|edit> [<variable> <value>] -- Tells current config.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "join <chan> -- Joins the bot to a channel");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "part <chan> [reason] -- Parts the bot from a channel");
            if (IsOwner(hostmask) || (check.isBotOp(nick) && (check.GetLevel(nick) >= 3))) DelayNotice(nick, prefix + "kick <nick> [reason] -- Kicks <nick> from the current channel for [reason], or, if [reason] is not specified, kicks user with one of the kick lines in the kicks database.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "kicklines <add|clear|read|total> <kickmessage|(do nothing)|number|(do nothing)> -- Does various actions to the kicklines database.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "reset -- Clears the config and restarts the bot");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "restart -- Restarts the bot");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "clean -- Clears the config and kills the bot");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "die [quitmessage] -- Kills the bot, with optional [quitmessage]");
            DelayNotice(nick, prefix + "time [<+|-> <number>] -- Tells the time in GMT/UTC, with the offset you specify.");
            DelayNotice(nick, prefix + "uptime -- Tells the time, which the bot is running now without crash or shutdown.");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "addbotop <nick> [level] -- Add a BotOp user, where nick is the nickname of the user you want to add and level is the access level of the user you want to add");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "delbotop <nick> -- Delete a BotOp user, where nick is the nickname of the user you want to Delete");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "amibotop -- Tells you if you are a BotOp");
            if (IsOwner(hostmask)) DelayNotice(nick, prefix + "setbotop <nick> <level> -- Sets the level of a BotOP");
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

        /// <summary>
        /// Sends ctcp data to someone.
        /// </summary>
        /// <param name="nickname">Target nickname</param>
        /// <param name="data">Data to send via CTCP</param>
        public void SendCTCP(string nickname, string data)
        {
            WriteData("PRIVMSG " + nickname + " :\x01" + data + "\x01");
        }


        /// <summary>
        /// Sends ctcp reply data to someone.
        /// </summary>
        /// <param name="nickname">Target nickname</param>
        /// <param name="data">Data to send via CTCP</param>
        public void SendCTCPReply(string nickname, string data)
        {
            WriteData("NOTICE " + nickname + " :\x01" + data + "\x01");
        }

        /// <summary>
        /// Validates an executable if it is usable in CSharpBot.
        /// </summary>
        /// <param name="fileName">The executable to check.</param>
        /// <returns>Boolean</returns>
        public bool CheckEXE(string fileName)
        {
            // read the bytes from the application exe file
            FileStream fs = new FileStream(fileName, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            byte[] bin = br.ReadBytes(Convert.ToInt32(fs.Length));
            fs.Close();
            br.Close();
            Assembly assem = Assembly.Load(bin);

            // search for the Entry Point
            MethodInfo method = assem.EntryPoint;

            if (method != null || method.Name == "CSBMain")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Lets the bot login as an operator. Also known as O-line.
        /// </summary>
        /// <param name="operuser">Oper-Username</param>
        /// <param name="operpass">Oper-Password</param>
        public void Oper(string operuser, string operpass)
        {
            WriteData("OPER " + operuser + " " + operpass);
        }

        /// <summary>
        /// Lets the bot login as a service
        /// </summary>
        /// <param name="nickname">Nickname of service</param>
        /// <param name="distribution">The filter for which users the service is visible.
        /// Examples: To allow all, just use "*". If you just want to allow German users, use "*.de".</param>
        /// <param name="info">Some info about the service</param>
        public void Service(string nickname, string distribution, string info)
        {
            WriteData("SERVICE " + nickname + " * " + distribution + " 0 0 :" + info);
        }

        /// <summary>
        /// [Only available to operators] Disconnect server link
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="comment">Comment</param>
        public void Squit(string server, string comment = "Forced disconnect")
        {
            WriteData("SQUIT " + server + " :" + comment);
        }

        /// <summary>
        /// Lets the server list all visible names in specific channels
        /// </summary>
        /// <param name="channels">The channels</param>
        public void Names(params string[] channels)
        {
            WriteData("NAMES " + string.Join(",", channels));
        }

        /// <summary>
        /// Lets the server list all visible names and their channels
        /// </summary>
        public void Names()
        {
            WriteData("NAMES");
        }

        /// <summary>
        /// Asks the server to list all channels and their topics
        /// </summary>
        public void List()
        {
            WriteData("LIST");
        }

        /// <summary>
        /// Asks the server to list specific channels and their topics
        /// </summary>
        public void List(params string[] channels)
        {
            WriteData("LIST " + string.Join(",", channels));
        }

        /// <summary>
        /// Lets the bot invite someone into a channel
        /// </summary>
        /// <param name="who">Who?</param>
        /// <param name="channel">From which channel?</param>
        public void Invite(string who, string channel)
        {
            WriteData("INVITE " + who + " " + channel);
        }

        /// <summary>
        /// Kick someone from a channel
        /// </summary>
        /// <param name="who">Who?</param>
        /// <param name="channel">From which channel?</param>
        public void Kick(string who, string channel)
        {
            WriteData("KICK " + who + " " + channel);
        }

        /// <summary>
        /// Kick someone from a channel with a reason
        /// </summary>
        /// <param name="who">Who?</param>
        /// <param name="channel">From which channel?</param>
        /// <param name="reason">Because of what?</param>
        public void Kick(string who, string channel, string reason)
        {
            WriteData("KICK " + who + " " + channel + " :" + reason);
        }

        /// <summary>
        /// Asks the server to print out its MOTD.
        /// </summary>
        public void MOTD()
        {
            WriteData("MOTD");
        }

        /// <summary>
        /// Asks the server to print out another server's MOTD.
        /// </summary>
        public void Motd(string server)
        {
            WriteData("MOTD " + server);
        }

        /// <summary>
        /// Asks the server to print out statistics about the size of the network
        /// </summary>
        /// <param name="mask"></param>
        public void Lusers()
        {
            WriteData("LUSERS");
        }

        /// <summary>
        /// Asks the server to print out statistics about the size of the network by a given mask
        /// </summary>
        /// <param name="mask">The mask</param>
        public void Lusers(string mask)
        {
            WriteData("LUSERS " + mask);
        }

        /// <summary>
        /// Asks the server to print out statistics about the size of a server by a given mask
        /// </summary>
        /// <param name="mask">The mask</param>
        public void Lusers(string mask, string target)
        {
            WriteData("LUSERS " + mask + " " + target);
        }

        /// <summary>
        /// Asks the server to print out the server version of another server
        /// </summary>
        /// <param name="target">The target server</param>
        public void Version(string target)
        {
            WriteData("VERSION " + target);
        }

        /// <summary>
        /// Asks the server to print out the server version
        /// </summary>
        public void Version(string target)
        {
            WriteData("VERSION");
        }

        /// <summary>
        /// Asks the server to print out statistics
        /// </summary>
        // TODO: Implement the other variations of STATS command.
        public void Stats()
        {
            WriteData("STATS");
        }

        // TODO: Implement BAN command
        // TODO: Implement the other RFC2812 commands. (http://www.faqs.org/rfcs/rfc2812.html)
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