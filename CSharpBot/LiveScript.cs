using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

                    //This marks the beginning of the identifiers with () in them ($calc(1+1))
                    if (identifier[1].Contains('(') && identifier[1].Contains(')'))
                    {
                        string[] temp1 = identifier[1].Split('(');
                        string[] temp = temp1[1].Split(')');
                        string args = temp[0];
                        switch (temp1[0])
                        {
                            case "calc":
                                try
                                {
                                    tmp = MathParser.Parse(args);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }
                                break;
                        }
                    }
                    else
                    {
                        //This marks the beginning of the identifiers without () in them ($channel)
                        switch (identifier[1])
                        {
                            case "channel":
                                tmp = CSharpBot.bot.currentchan;
                                break;
                            case "time":
                                tmp = DateTime.Now.ToString("h:mm tt");
                                break;
                        }
                    }
                    codeout = codeout + tmp + " ";
                }
                else
                {
                    codeout = codeout + s + " ";  
                }
            }
            return codeout;
        }

        public void RunScript(string input)
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
