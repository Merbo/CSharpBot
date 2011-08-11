using System;
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
            string[] codesplit = code.Split(';');
         
            foreach (string cmd in codesplit)
            {
                string[] tmp = cmd.Split(' ');
                if (cmd.StartsWith("msg ") || cmd.StartsWith(" msg "))
                {
                    if (cmd.StartsWith(" msg "))
                    {
                        CSharpBot.bot.Functions.PrivateMessage(tmp[2], string.Join(" ", tmp.Skip(3)));
                    }
                    else
                    {
                        CSharpBot.bot.Functions.PrivateMessage(tmp[1], string.Join(" ", tmp.Skip(2)));
                    }
                }
                if (cmd.StartsWith("notice ") || cmd.StartsWith(" notice "))
                {
                    if (cmd.StartsWith(" notice "))
                    {
                        CSharpBot.bot.Functions.Notice(tmp[2], string.Join(" ", tmp.Skip(3)));
                    }
                    else
                    {
                        CSharpBot.bot.Functions.Notice(tmp[1], string.Join(" ", tmp.Skip(2)));
                    }
                }
                else if (cmd == "die" || cmd == " die")
                {
                    CSharpBot.bot.Shutdown();
                }
            }
        }
    }
}
