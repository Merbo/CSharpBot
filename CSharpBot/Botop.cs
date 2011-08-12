using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
   public class Botop
    {
        BotOpDB conn;

	public Botop()
	{
	    /** FIXME: Not cross-platform-compatible: BotOpDB
	     *
	     * The file-based database connection is currently not supported
             * by Mono. This problem has been found by accident.
             *
             * Exception: NotSupportedException
             */
            if(!Environment.OSVersion.Platform.ToString().ToLower().Contains("win"))
                Console.WriteLine("WARNING: BotOp database not usable on non-Windows systems.");
            else
	        conn = new BotOpDB("Data Source=BotOpDB.sdf");
	}

        public void AddBotOp(string nickname, int level=5)
        {
            Nicks newnick = new Nicks();
            newnick.AccessLevel = level;
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

        public int GetLevel(string nickname)
        {
            var q = from c in conn.Nicks
                    where c.Nick == nickname
                    select c;
            int level = 0;
            foreach (var c in q)
            {                          //Statements in here only executed if match found
                level = c.AccessLevel.Value;

            }
            return level;
         
        }

        public bool SetLevel(string nickname, int level)
        {
            var q = from c in conn.Nicks
                    where c.Nick == nickname
                    select c;
            bool wasin = false;
            foreach (var c in q)
            {                          //Statements in here only executed if match found
                c.AccessLevel = level;
                conn.SubmitChanges();
                wasin = true;
            }
            return wasin;

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
