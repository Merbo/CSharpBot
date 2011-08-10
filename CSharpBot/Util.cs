using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    public static class StringHelper
    {
        static public bool EqualsAny(this string sh, string[] sts) {
            foreach (string s in sts) {
                if (sh.Equals(s))
                    return true;
            }
            return false;
        }
    }

    public static class CharHelper
    {
        /// <summary>
        /// True if this char is equal to any characters in the specified
        /// character array. 
        /// </summary>
        /// <param name="ch">char to compare. </param>
        /// <param name="ca">char array to compare against. </param>
        static public bool EqualsAny(this char ch, char[] ca) {
            return ca.Any(ch.Equals);
        }
    }
}
