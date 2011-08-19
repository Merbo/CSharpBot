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
        public static string read(StreamReader reader)
        {
            try
            {
                return reader.ReadLine();
            }
            catch
            {
                Console.WriteLine("Unable to read from server");
                return null;
            }
        }
        public static string read()
        {
            try
            {
                return Client.reader.ReadLine();
            }
            catch
            {
                Console.WriteLine("Unable to read from server");
                return null;
            }
        }

        public static void write(string data, StreamWriter writer)
        {
            try
            {
                writer.WriteLine(Client.nick + " " + data);
                //Console.WriteLine("-->  " + data);
                writer.Flush();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error writing: " + e.Message);
            }
        }
        public static void write(string data)
        {
            try
            {
                Client.writer.WriteLine(data);
                //Console.WriteLine("--> " + data);
                Client.writer.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error writing: " + e.Message);
            }
        }
    }
}
