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
            string[] split = codein.Split(' ');
            string codeout = "";
            string tmp = "";
            foreach (string s in split)
            {
                if (s.StartsWith("$"))
                {
                    string sa = s.ToLower();
                    string[] identifier = sa.Split('$');
                    //This marks the beginning of the identifiers without () in them ($channel)
                    if (identifier[1].Equals("channel"))
                        tmp = CSharpBot.bot.currentchan;
                    else if (identifier[1].Equals("time"))
                        tmp = DateTime.Now.ToString("h:mm tt");
                    //This marks the beginning of the identifiers with () in them ($calc(1+1))
                    else if (identifier[1].EndsWith("(") && identifier[1].EndsWith(")"))
                    {
                        string[] temp1 = identifier[1].Split('(');
                        string[] temp = temp1[1].Split(')');
                        if (temp1[0].Equals("calc"))
                        {
                            try
                            {
                                tmp = MathParser.Parse(temp[0]);
                            }
                            catch (Exception)
                            {
                                //Nothing.
                            }
                        }
                    }  
                    codeout = codeout + tmp;
                }
                else
                {
                    codeout = codeout + s;  
                }
            }
            return codeout;
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
