﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpBot
{
    /// <summary>
    /// Contains all reply codes defined in RFC 2812
    /// </summary>
    // Source: http://www.mirc.com/rfc2812.html
    public enum IrcReplyCode
    {
        // reply codes
        RPL_WELCOME = 001,
        RPL_YOURHOST = 002,
        RPL_CREATED = 003,
        RPL_MYINFO = 004,
        RPL_BOUNCE = 005,

        RPL_USERHOST = 302,
        RPL_ISON = 303,
        RPL_AWAY = 301,
        RPL_UNAWAY = 305,
        RPL_NOWAWAY = 306,

        RPL_WHOISUSER = 311,
        RPL_WHOISSERVER = 312,
        RPL_WHOISOPERATOR = 313,
        RPL_WHOISIDLE = 317,
        RPL_ENDOFWHOIS = 318,
        RPL_WHOISCHANNELS = 319,

        RPL_WHOWASUSER = 314,
        RPL_ENDOFWHOWAS = 369,

        [Obsolete("Not used", false)]
        RPL_LISTSTART = 321,
        RPL_LIST = 322,
        RPL_LISTEND = 323,

        RPL_UNIQOPIS = 325,
        RPL_CHANNELMODEIS = 324,

        RPL_NOTOPIC = 331,
        RPL_TOPIC = 332,

        RPL_INVITING = 341,

        RPL_SUMMONING = 342,

        RPL_INVITELIST = 346,
        RPL_ENDOFINVITELIST = 347,

        RPL_VERSION = 351,

        RPL_WHOREPLY = 352,
        RPL_ENDOFWHO = 315,

        RPL_NAMREPLY = 353,
        RPL_ENDOFNAMES = 366,

        RPL_LINKS = 364,
        RPL_ENDOFLINKS = 365,

        RPL_BANLIST = 367,
        RPL_ENDOFBANLIST = 368,

        RPL_INFO = 371,
        RPL_ENDOFINFO = 374,

        RPL_MOTDSTART = 375,
        RPL_MOTD = 372,
        RPL_ENDOFMOTD = 376,

        RPL_YOUREOPER = 381,
        RPL_REHASHING = 382,
        RPL_YOURESERVICE = 383,

        RPL_TIME = 391,

        RPL_USERSSTART = 392,
        RPL_USERS = 393,
        RPL_ENDOFUSERS = 394,
        RPL_NOUSERS = 395,

        RPL_TRACELINK = 200,
        RPL_TRACECONNECTING = 201,
        RPL_TRACEHANDSHAKE = 202,
        RPL_TRACEUNKNOWN = 203,
        RPL_TRACEOPERATOR = 204,
        RPL_TRACEUSER = 205,
        RPL_TRACESERVER = 206,
        RPL_TRACESERVICE = 207,
        RPL_TRACENEWTYPE = 208,
        RPL_TRACECLASS = 209,
        [Obsolete("Unused", false)]
        RPL_TRACERECONNECT = 210,
        RPL_TRACELOG = 261,
        RPL_TRACEEND = 262,

        RPL_STATSLINKINFO = 211,
        RPL_STATSCOMMANDS = 212,
        RPL_ENDOFSTATS = 219,

        RPL_STATSUPTIME = 242,
        RPL_STATSOLINE = 243,

        RPL_UMODEIS = 221,

        RPL_SERVLIST = 234,
        RPL_SERVLISTEND = 235,

        RPL_LUSERCLIENT = 251,
        RPL_LUSEROP = 252,
        RPL_LUSERUNKNOWN = 253,
        RPL_LUSERCHANNELS = 254,
        RPL_LUSERME = 255,

        RPL_ADMINME = 256,
        RPL_ADMINLOC1 = 257,
        RPL_ADMINLOC2 = 258,
        RPL_ADMINEMAIL = 259,

        RPL_TRYAGAIN = 263,

        // error codes
        ERR_NOSUCHNICK = 401,
        ERR_NOSUCHSERVER = 402,
        ERR_NOSUCHCHANNEL = 403,
        ERR_CANNOTSENDTOCHAN = 404,
        ERR_TOOMANYCHANNELS = 405,
        ERR_WASNOSUCHNICK = 406,
        ERR_TOOMANYTARGETS = 407,
        ERR_NOSUCHSERVICE = 408,
        ERR_NOORIGIN = 409,
        ERR_NORECIPIENT = 411,
        ERR_NOTEXTTOSEND = 412,
        ERR_NOTOPLEVEL = 413,
        ERR_WILDTOPLEVEL = 414,
        ERR_BADMASK = 415,
        ERR_UNKNOWNCOMMAND = 421,
        ERR_NOMOTD = 422,
        ERR_NOADMININFO = 423,
        ERR_FILEERROR = 424,
        ERR_NONICKNAMEGIVEN = 431,
        ERR_ERRONEUSNICKNAME = 432,
        ERR_NICKNAMEINUSE = 433,
        ERR_NICKCOLLISION = 436,
        ERR_UNAVAILRESOURCE = 437,
        ERR_USERNOTINCHANNEL = 441,
        ERR_NOTONCHANNEL = 442,
        ERR_USERONCHANNEL = 443,
        ERR_NOLOGIN = 444,
        ERR_SUMMONDISABLED = 445,
        ERR_USERSDISABLED = 446,
        ERR_NOTREGISTERED = 451,
        ERR_NEEDMOREPARAMS = 461,
        ERR_ALREADYREGISTERED = 462,
        ERR_NOPERMFORHOST = 463,
        ERR_PASSWDMISMATCH = 464,
        ERR_YOUREBANNEDCREEP = 465,
        ERR_YOUWILLBEBANNED = 466,
        ERR_KEYSET = 467,
        ERR_CHANNELISFULL = 471,
        ERR_UNKNOWNMODE = 472,
        ERR_INVITEONLYCHAN = 473,
        ERR_BANNEDFROMCHAN = 474,
        ERR_BADCHANNELKEY = 475,
        ERR_BADCHANMASK = 476,
        ERR_NOCHANMODES = 477,
        ERR_BANLISTFULL = 478,
        ERR_NOPRIVILEGES = 481,
        ERR_CHANOPRIVSNEEDED = 482,
        ERR_CANTKILLSERVER = 483,
        ERR_RESTRICTED = 484,
        ERR_UNIQOPPRIVSNEEDED = 485,
        ERR_NOOPERHOST = 491,
        ERR_UMODEUNKNOWNFLAG = 501,
        ERR_USERSDONTMATCH = 402,

        /// <remarks>This is a non-generic code</remarks>
        RPL_SERVICEINFO = 231,
        /// <remarks>This is a non-generic code</remarks>
        RPL_SERVICE = 233,
        /// <remarks>This is a non-generic code</remarks>
        RPL_ENDOFSERVICES = 232,

        /// <remarks>This is a non-generic code</remarks>
        RPL_NONE = 300, // lol
        /// <remarks>This is a non-generic code</remarks>
        RPL_WHOISCHANOP = 316,
        /// <remarks>This is a non-generic code</remarks>
        RPL_KILLDONE = 361,
        /// <remarks>This is a non-generic code</remarks>
        RPL_CLOSING = 362,
        /// <remarks>This is a non-generic code</remarks>
        RPL_CLOSEEND = 363,
        /// <remarks>This is a non-generic code</remarks>
        RPL_INFOSTART = 373,
        /// <remarks>This is a non-generic code</remarks>
        RPL_MYPORTIS = 384,

        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSCLINE = 213,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSNLINE = 214,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSILINE = 215,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSKLINE = 216,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSQLINE = 217,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSYLINE = 218,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSVLINE = 240,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSLLINE = 241,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSHLINE = 244,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSSLINE = 245, // they did hopefully mean this, since RFC 2812 says, it's 244 also, but 245 isn't used (which is pretty senseless) :-/
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSPING = 246,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSBLINE = 247,
        /// <remarks>This is a non-generic code</remarks>
        RPL_STATSDLINE = 250,

        /// <remarks>This is a non-generic code</remarks>
        ERR_NOSERVICEHOST = 492,
        /// <remarks>This is a non-generic code</remarks>
        ERR_NEEDTOBEREGISTERED = 486,

        /// <remarks>This is a non-standard code for StartTLS</remarks>
        RPL_STARTTLSSUCCESSFUL = 670,
        
        /// <remarks>This is a non-standard code for StartTLS</remarks>
        ERR_STARTTLSFAILED = 691
    }
}