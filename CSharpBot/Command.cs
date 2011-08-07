#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    /// <summary>
    /// Defines a command, or any response from an IRC server, sent to the bot. 
    /// </summary>
    public class Command
    {
        #region Private Fields

        private string[] _cmd;
        private string _pf = ":";

        #endregion

        #region Properties
        public bool IsPing {
            get { return _cmd[0].Equals("PING"); }
        }

        public bool IsEndMotd {
            get { return _cmd[1].Equals("376"); }
        }

        public bool IsWhoisRequest {
            get { return _cmd[1].Equals("311"); }
        }

        public bool IsBotCommand {
            get {
                return _cmd[3].Equals(_pf + "test", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "amiowner", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "time", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "mynick", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "myident", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "myhost", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "myfullmask", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "die", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "clean", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "topic", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "kicklines", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "kick", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "join", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "help", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "mode", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "part", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "reset", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "config", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "restart", StringComparison.OrdinalIgnoreCase) ||
                       _cmd[3].Equals(_pf + "hostmask", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// The hostmask of the source of this IRC command, if present. 
        /// </summary>
        public string HostMask {
            get {
                if (_cmd[0] != null)
                    return _cmd[0];
                return null;
            }
        }

        /// <summary>
        /// The IRC command string, if present. 
        /// </summary>
        public string CommandString {
            get {
                if (_cmd[1] != null)
                    return _cmd[1];
                return null;
            }
        }

        /// <summary>
        /// The source or reciever of this IRC command, if present. 
        /// </summary>
        public string Target {
            get {
                if (_cmd[2] != null)
                    return _cmd[2];
                return null;
            }
        }

        /// <summary>
        /// The message of this IRC command, if present. 
        /// </summary>
        public string Message {
            get {
                if (_cmd[3] != null)
                    return _cmd[3];
                return null;
            }
        }
        #endregion

        #region Methods
        public Command(string[] cmd) {
            _cmd = cmd;
            if (_pf.Equals(":"))
                _pf += Program.config.Prefix;
            if (Program.DEBUG) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("<= : Command, " + HostMask + ", " + CommandString +
                                  ", " + Target + ", " + Message + ".");
                Console.ResetColor();
            }
        }
        #endregion
    }
}