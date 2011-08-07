using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace CSharpBot
{
    class Functions
    {
        /// <summary>
        /// Checks if a hostmask fits to the owner's hostmask regex.
        /// </summary>
        /// <param name="inputmask"></param>
        /// <returns></returns>
        public static bool IsOwner(string inputmask)
        {
            return Program.HostmaskRegex.Match(inputmask).Success;
        }
        public static void Log(string input)
        {
            if (Program.logging == true)
            {
                string f = Program.config.Logfile;
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
        public static void WriteToFile(string file, string input)
        {
            if (Program.logging == true)
            {
                if (File.Exists(file))
                {
                    List<string> lines = new List<string>();
                    using (StreamReader r = new StreamReader(file))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                    using (StreamWriter w = new StreamWriter(file))
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
                    using (StreamWriter w = new StreamWriter(file))
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
        public static void Say(string channel, string text)
        {
            if (channel.StartsWith("#"))
            {
                Program.writer.WriteLine("PRIVMSG " + channel + " :" + text);
            }
        }
        public static void SendHelp(object o)
        {
            string[] param = (string[])o;
            string nick = param[0];
            string prefix = param[1];
            Program.writer.WriteLine("NOTICE " + nick + " :Bot commands:");
            Program.writer.WriteLine("NOTICE " + nick + " :Everything in <> is necessary and everything in [] are optional.");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "help -- This command.");
            Thread.Sleep(1000);
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "mode <mode>-- Sets a mode the current channel.");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "topic [topic] -- Tells the current topic OR sets the channel's topic to [topic]");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "config <list|edit> [<variable> <value>] -- Tells current config.");
            Thread.Sleep(1000);
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "join <chan> -- Joins the bot to a channel");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "part <chan> [reason] -- Parts the bot from a channel");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "kick <nick> [reason] -- Kicks <nick> from the current channel for [reason], or, if [reason] is not specified, kicks user with one of the kick lines in the kicks database.");
            Thread.Sleep(1000);
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "kicklines <add|clear|read|total> <kickmessage|(do nothing)|number|(do nothing)> -- Does various actions to the kicklines database.");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "reset -- Clears the config and restarts the bot");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "restart -- Restarts the bot");
            Thread.Sleep(1000);
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "clean -- Clears the config and kills the bot");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "die [quitmessage] -- Kills the bot, with optional [quitmessage]");
            Program.writer.WriteLine("NOTICE " + nick + " :" + prefix + "time [<+|-> <number>] -- Tells the time in GMT/UTC, with the offset you specify.");
        }
        public static void WriteData(string data)
        {
            Program.writer.WriteLine(data);
            if (Program.DEBUG == true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("SEND: " + data);
                Console.ResetColor();
            }
        }
    }
}
