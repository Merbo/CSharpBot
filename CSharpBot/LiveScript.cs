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
            string codeout = codein.Replace("$CHANNEL", CSharpBot.bot.currentchan);
            return codeout;
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
                else if (cmd == "time" || cmd == " time")
                {
                    CSharpBot.bot.Functions.PrivateMessage(CSharpBot.bot.currentchan, DateTime.Now.ToString("h:mm tt"));
                }
                else if (cmd == "die" || cmd == " die")
                {
                    CSharpBot.bot.Shutdown();
                }
            }
        }
    }
}
