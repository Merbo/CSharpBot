﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    class LiveScript
    {

        public LiveScript() {}
        string stripnewlines(string codein)
        {
            string codeout;
            codeout = codein.Replace("\r\n", "").Replace("\n", "").Replace("\r", ""); //Parser Doesn't take newlines, so we remove all newlines
            return codeout;
        }

        string processvars(string codein)
        {
            string cin = codein.ToLower();
            string identifier = cin;
            identifier = identifier.Replace("$channel", CSharpBot.bot.currentchan);
            identifier = identifier.Replace("$time", DateTime.Now.ToString("h:mm tt"));
            return identifier;
        }
      public  void RunScript(string input)
        {


            string code = input;
            code = stripnewlines(code);
            code = processvars(code);
         
         
            foreach (string cmd in code.Split(';'))
            {

                if (cmd.StartsWith("print ") || cmd.StartsWith(" print "))
                {
                    int sub = 6;
                    if (cmd.StartsWith(" print "))
                        sub = 7;
                    CSharpBot.bot.Functions.PrivateMessage(CSharpBot.bot.currentchan, cmd.Substring(sub));
                }
                else if (cmd == "die" || cmd == " die")
                {
                    CSharpBot.bot.Shutdown();
                }
            }
        }
    }
}
