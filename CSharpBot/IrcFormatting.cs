using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    public static class IrcFormatting
    {
        #region IRC formatting

        const string IRCBold = "\x02"; // \x02[text]\x02
        const string IRCColor = "\x03"; // \x03[xx[,xx]]
        const string IRCReversed = "\u0016"; // Some may interpret this as Italic, but Mibbit has a bug on this
        const string IRCUnderlined = "\u001F";
        const string IRCReset = "\u000F"; // Resets text formatting

        /// <summary>
        /// Outputs underlined IRC-formatted text
        /// </summary>
        /// <param name="text">The text</param>
        /// <returns>IRC-formatted text</returns>
        public static string UnderlinedText(string text) { return IRCUnderlined + text + IRCUnderlined; }

        /// <summary>
        /// Outputs bold IRC-formatted text
        /// </summary>
        /// <param name="text">The text</param>
        /// <returns>IRC-formatted text</returns>
        public static string BoldText(string text) { return IRCBold + text + IRCBold; }

        /// <summary>
        /// Outputs reversed/italic IRC-formatted text
        /// </summary>
        /// <param name="text">The text</param>
        /// <returns>IRC-formatted text</returns>
        public static string ReversedText(string text) { return IRCReversed + text + IRCReversed; }

        /// <summary>
        /// Outputs colored IRC-formatted text
        /// </summary>
        /// <param name="text">The text</param>
        /// <returns>IRC-formatted text</returns>
        /// <param name="foreground">The foreground color</param>
        /// <param name="background">The background color</param>
        public static string ColorText(string text, IrcColor foreground, IrcColor background = IrcColor.Reset)
        {
            return IRCColor
                + GetColorCode2Digits(foreground)
                + (background != IrcColor.Reset
                    ? "," + GetColorCode2Digits(foreground)
                    : ""
                  )
                + text
                + IRCColor
                + GetColorCode2Digits(IrcColor.Reset);
        }

        /// <summary>
        /// Returns the 2-digit code of the color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private static string GetColorCode2Digits(IrcColor color) { string n = GetColorCodeInt(color).ToString(); while (n.Length < 2) n = "0" + n; return n; }
        private static int GetColorCodeInt(IrcColor color) { return (int)color; }
        private static string GetColorCodeName(IrcColor color) { return color.ToString(); }
        private static string GetColorCodeName(int color) { return ((IrcColor)color).ToString(); }
        #endregion
    }

    public enum IrcColor
    {
        // imported from http://www.mirc.co.uk/colors.html
        // for comfort, we're using multiple names for the color codes.
        White = 0,
        Black = 1,
        Blue = 2,
        NavyBlue = 2,
        Green = 3,
        Red = 4,
        Brown = 5,
        Maroon = 5,
        Purple = 6,
        Orange = 7,
        Olive = 7,
        Yellow = 8,
        LimeGreen = 9,
        LightGreen = 10,
        Teal = 10,
        Cyan = 11,
        Aqua = 11,
        LightCyan = 11,
        RoyalBlue = 12,
        LightBlue = 12,
        LightPurple = 13,
        Pink = 13,
        Grey = 14,
        LightGrey = 15,
        Silver = 15,
        Reset = 99
    }
}
