using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Client
{
    public class Functions
    {
        public static bool Debug = Client.Debug;


        /// <summary>
        /// Used for writing data to the console, the easy way.
        /// </summary>
        /// <param name="data">The text to write to the console.</param>
        /// <param name="level">Level is a special parameter.
        ///() Means necessary parameter
        ///[] Means optional parameter
        ///
        ///Syntax: Log((string), [int])
        ///
        ///Action: Writes (string) to the console, also can use presets based off of simple tasks, via use of [int]
        ///
        ///By default, this will simply do a white console.WriteLine of (string).
        ///However, [int] can be:
        ///0: Error
        ///1: Warning
        ///2: Notice
        ///3: Debug*
        ///4: Log*
        ///5: Setup**
        ///6: Plain old Console.WriteLine
        ///(*: Only runs if 'Debug' evaluates to true)
        ///(**: Only meant for use at the beginning setup configuration.)
        /// </param>
        public static void Log(string data, int level = 6)
        {
            switch (level)
            {
                //our priority is based off of the height of our number.
                //For example, 0 is a huge exception, but 3 might be a debug message.
                //However, 5 is for setup ONLY, and 6 is an alias for Console.WriteLine.
                case 0:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR: " + data);
                    Console.ResetColor();
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("WARNING: " + data);
                    Console.ResetColor();
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("NOTICE: " + data);
                    Console.ResetColor();
                    break;
                case 3:
                    if (Debug)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("DEBUG: " + data);
                        Console.ResetColor();
                    }
                    break;
                case 4:
                    if (Debug)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("LOG: " + data);
                        Console.ResetColor();
                    }
                    break;
                //exception from leveling: Setup.
                case 5:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.Write("SETUP: " + data);
                    Console.ResetColor();
                    break;
                //exception from leveling: Simple Console.WriteLine.
                case 6:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(data);
                    Console.ResetColor();
                    break;
            }
        }

        /// <summary>
        /// Reads a line from a specified StreamReader.
        /// </summary>
        /// <param name="reader">StreamReader to read from.</param>
        /// <returns>Returns what it read from the StreamReader, a string.</returns>
        public static string read(StreamReader reader)
        {
            try
            {
                string line = reader.ReadLine();
                Log("<-- " + line);
                return line;
            }
            catch
            {
               Log("Unable to read from server", 0);
                return null;
            }
        }
        
        /// <summary>
        /// Reads from the default StreamReader.
        /// </summary>
        /// <returns>Returns 1 line from the default StreamReader.</returns>
        public static string read()
        {
            try
            {
                string line = Client.reader.ReadLine();
                Log("<-- " + line);
                return line; 
            }
            catch
            {
                Log("Unable to read from server", 0);
                return null;
            }
        }

        /// <summary>
        /// Writes specified text using a specified StreamWriter.
        /// </summary>
        /// <param name="data">Text to write.</param>
        /// <param name="writer">StreamWriter to use.</param>
        public static void write(string data, StreamWriter writer)
        {
            try
            {
                writer.WriteLine(Client.nick + " " + data);
                Log("-->  " + data, 3);
                writer.Flush();
            }
            catch
            {
                Console.WriteLine("Error!");
            }
        }

        /// <summary>
        /// Writes text via the default StreamReader.
        /// </summary>
        /// <param name="data">Text to write.</param>
        public static void write(string data)
        {
            try
            {
                Client.writer.WriteLine(Client.nick + " " + data);
                Log("--> " + data, 3);
                Client.writer.Flush();
            }
            catch
            {
                Console.WriteLine("Error!");
            }
        }
    }
}
