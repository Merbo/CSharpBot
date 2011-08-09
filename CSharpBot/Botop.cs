using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
   public class Botop
    {
        BotOpDB conn = new BotOpDB("Data Source=BotOpDB.sdf");

        public void AddBotOp(string nickname)
        {
            Nicks newnick = new Nicks();
            newnick.Nick = nickname;
            conn.Nicks.InsertOnSubmit(newnick);
            conn.SubmitChanges();
        }

        public bool isBotOp(string nickname)
        {
            var q = from c in conn.Nicks
                    where c.Nick == nickname
                    select c;
            bool isBO = false;
            foreach (var c in q)
            {                          //Statements in here only executed if match found
                isBO = true;
            }
            return isBO;
        }


        public void DelBotOp(string nickname)
        {
            var q = from c in conn.Nicks
                    where c.Nick == nickname
                    select c;
            foreach (var c in q)
            {
                conn.Nicks.DeleteOnSubmit(c);
                conn.SubmitChanges();
            }
        }
       
                               
    }
}
