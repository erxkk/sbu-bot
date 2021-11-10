using System;
using System.Collections.Generic;
using System.Linq;

namespace SbuBot
{
    public static class SbuGlobals
    {
        public const string BULLET = "•";
        public const string ELLIPSES = "…";
        public const string DESCRIPTOR_SEPARATOR = "::";
        public const string DEFAULT_PREFIX = "sbu";
        public const string DEV_PREFIX = "dev";

        public static class Keyword
        {
            public static readonly IReadOnlySet<string> ALL_RESERVED;
            public static readonly IReadOnlySet<string> IDENTIFIERS;
            public static readonly IReadOnlySet<string> CONTROL_FLOW;
            public static readonly IReadOnlyDictionary<string, string[]> COMMAND_ALIASES;

            static Keyword()
            {
                SbuGlobals.Keyword.IDENTIFIERS = new HashSet<string> { "all", "none", "mine", "reserved" };
                SbuGlobals.Keyword.CONTROL_FLOW = new HashSet<string> { "abort" };

                SbuGlobals.Keyword.COMMAND_ALIASES = new Dictionary<string, string[]>
                {
                    ["help"] = new[] { "h", "?" },
                    ["claim"] = new[] { "take" },
                    ["create"] = new[] { "make", "mk" },
                    ["set"] = Array.Empty<string>(),
                    ["get"] = Array.Empty<string>(),
                    ["edit"] = new[] { "change" },
                    ["list"] = new[] { "ls" },
                    ["delete"] = new[] { "remove", "rm" },
                    ["transfer"] = new[] { "move", "mv" },
                };

                SbuGlobals.Keyword.ALL_RESERVED = SbuGlobals.Keyword.COMMAND_ALIASES
                    .SelectMany(e => e.Value.Append(e.Key))
                    .Append(SbuGlobals.BULLET)
                    .Append(SbuGlobals.ELLIPSES)
                    .Append(SbuGlobals.DESCRIPTOR_SEPARATOR)
                    .Append(SbuGlobals.DEFAULT_PREFIX)
                    .Concat(SbuGlobals.Keyword.CONTROL_FLOW)
                    .Concat(SbuGlobals.Keyword.IDENTIFIERS)
                    .ToHashSet();
            }
        }

        public static class Link
        {
            public const string GH_SELF = "https://github.com/erxkk/sbu-bot";
            public const string GH_DISQORD = "https://github.com/quahu/disqord";
        }

        public static class Guild
        {
            public const ulong SBU = 732210852849123418UL;
            public const ulong LA_FAMILIA = 681285811945340959UL;
            public const ulong EMOTES = 846789266041470977UL;
        }

        public static class Role
        {
            // top roles
            public const ulong ADMIN = 741033629613948961UL;
            public const ulong SBU_BOT = 851039575476273193UL;
            public const ulong BOTS = 732228987366932551UL;

            // separators
            public const ulong COLOR_SEPARATOR = 753986150724534404UL;

            // notable permission roles
            public const ulong MUTED = 772062437490294784UL;
            public const ulong NO_SERIOUS = 776789839793618984UL;
            public const ulong SENATE = 775428028771598346UL;
            public const ulong SHIT_SBU_SAYS = 759189942730883083UL;
            public const ulong ANNOUNCEMENTS = 773225941551153182UL;
            public const ulong BOOSTER = 732232124714713229UL;
            public const ulong PIN = 817448908095488000UL;
        }

        public static class Channel
        {
            // top level
            public const ulong HELP = 732210852849123421UL;
            public const ulong ANNOUNCEMENTS = 732231139233759324UL;
            public const ulong SENATE = 775427903206457365UL;

            // categories
            public const ulong CATEGORY_SERIOUS = 775427903206457365UL;

            // notable channels
            public const ulong PIN_ARCHIVE = 775826206410407937UL;
            public const ulong SHIT_SBU_SAYS = 759153822806310912UL;
            public const ulong CRUMPET_SERVER = 790542075346944020UL;
        }

        // saved on different guild to make sure they exist when used
        public static class Emote
        {
            public const ulong CUM = 863031236985356319UL;

            public static class Vote
            {
                public const ulong UP = 854440379948466176UL;
                public const ulong DOWN = 854440380031565834UL;
                public const ulong NONE = 854441334852943903UL;
            }

            public static class Menu
            {
                public const ulong BACK = 863135848828108811UL;
                public const ulong FAST_BACK = 863141104559456286UL;
                public const ulong FORWARD = 863139842581069864UL;
                public const ulong FAST_FORWARD = 863141104567058442UL;
                public const ulong CONFIRM = 864823489948680202UL;
                public const ulong STOP = 863139842748710962UL;
            }
        }

        public static class Users
        {
            public const ulong BOT = 849571821930283018UL;
            public const ulong DEV_BOT = 710538840728928327UL;
            public const ulong ERXKK = 356017256481685506UL;
            public const ulong DM = 675056356717232169UL;
            public const ulong TOASTY = 145652148548403200UL;
        }
    }
}