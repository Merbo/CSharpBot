using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    [Serializable]
    public class IrcException : Exception
    {
            public IrcException()
        : base() { }
    
    public IrcException(string message)
        : base(message) { }
    
    public IrcException(string format, params object[] args)
        : base(string.Format(format, args)) { }
    
    public IrcException(string message, Exception innerException)
        : base(message, innerException) { }
    
    public IrcException(string format, Exception innerException, params object[] args)
        : base(string.Format(format, args), innerException) { }

    }
}
