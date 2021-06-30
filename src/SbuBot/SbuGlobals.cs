using System;

namespace SbuBot
{
    public static class SbuGlobals
    {
        public const char BULLET = '•';
        public const char ELLIPSES = '…';
        public const char DESCRIPTOR_SEPARATOR = '|';
        public const string DEFAULT_PREFIX = "sbu";

        public static readonly Version VERSION = new(0, 4, 8);

        public static readonly string[] RESERVED_KEYWORDS =
        {
            // common command names
            "help", "claim", "take", "create", "make", "new", "list", "delete", "remove", "transfer",

            // identifiers
            "all", "none", "mine", "reserved", "cancel",

            // control flow
            "abort", "yes", "no",
        };

        public static class Github
        {
            public const string SELF = "https://github.com/erxkk/sbu-bot";
            public const string DISQORD = "https://github.com/quahu/disqord";
        }

        public static class Bot
        {
            public const ulong SELF = 849571821930283018UL;
            public const ulong OWNER = 356017256481685506UL;
            public const string NAME = "Allah 2";
            public const ushort DISCRIMINATOR = 1552;
        }

        public static class Guild
        {
            public const ulong SELF = 732210852849123418UL;
            public const ulong OWNER = 356017256481685506UL;
            public const string NAME = "Siege Bad Uninstall";
        }

        public static class Role
        {
            public const ulong ADMIN = 741033629613948961UL;
            public const ulong SBU_BOT = 851039575476273193UL;
            public const ulong BOTS = 732228987366932551UL;

            public static class Perm
            {
                public const ulong SELF = 776790664436383794UL;
                public const ulong MUTED = 772062437490294784UL;
                public const ulong NO_SERIOUS = 776789839793618984UL;
                public const ulong SENATE = 775428028771598346UL;
                public const ulong SHIT_SBU_SAYS = 759189942730883083UL;
                public const ulong ANNOUNCEMENTS = 773225941551153182UL;
                public const ulong BOOSTER = 732232124714713229UL;
                public const ulong PIN = 817448908095488000UL;
            }

            public static class Mention
            {
                public const ulong SELF = 732238126998880267UL;
                public const ulong BABANER = 824407962218135584UL;
                public const ulong MEME_KING = 812605553494720533UL;
            }

            public static class Color
            {
                public const ulong SELF = 753986150724534404UL;
            }
        }

        public static class Channel
        {
            public const ulong HELP = 732210852849123421UL;
            public const ulong ANNOUNCEMENTS = 732231139233759324UL;
            public const ulong SENATE = 775427903206457365UL;
            public const ulong ADMIN_FURRY_STASH = 837447382441656320UL;

            public static class Normal
            {
                public const ulong SELF = 732210852849123419UL;
                public const ulong GENERAL = 732211844315349005UL;
                public const ulong GAMING = 732211184043819041UL;
                public const ulong MEME = 732212585910370374UL;
                public const ulong WHOLESOME = 732213515909070889UL;
                public const ulong BOT_SPAM = 732229179889942539UL;
            }

            public static class Based
            {
                public const ulong SELF = 760469720741969950UL;
                public const ulong PIN_ARCHIVE = 775826206410407937UL;
                public const ulong SHIT_SBU_SAYS = 759153822806310912UL;
                public const ulong CRUMPET_SERVER = 790542075346944020UL;
                public const ulong DUDE_ZONE = 808295761841225739UL;
                public const ulong SOCIETY = 732212629476737095UL;
            }

            public static class Serious
            {
                public const ulong SELF = 732215815537164309UL;
                public const ulong ART = 732211271075889243UL;
                public const ulong MUSIC = 732212660258472027UL;
                public const ulong FILM = 764183510357245983UL;
                public const ulong FINANCE = 785518480086007819UL;
                public const ulong TECH = 786871336764571679UL;
                public const ulong VENTING = 732215756556730431UL;
            }

            public static class Voice
            {
                public const ulong SELF = 732210852849123420UL;
                public const ulong TEXT = 798227185059627009UL;
                public const ulong GENERAL = 732210852849123422UL;
                public const ulong GAMING = 732210939402649660UL;
                public const ulong GAMING_2 = 732211068256125008UL;
            }

            public static class Deleted
            {
                public const ulong SELF = 784093487150399508UL;
                public const ulong CHESS = 818919984470294578UL;
                public const ulong GAMING_SALES = 758612906778165288UL;
                public const ulong INVERSE_CUM_ROOM = 760468275750240266UL;
                public const ulong CUM_ROOM = 760468335439380521UL;
                public const ulong CYBERPUNK = 786519547225833482UL;
                public const ulong DEGENERATE = 732211770613170246UL;
                public const ulong WHEN_THE = 843720840139112449UL;
            }
        }

        // saved on different guild to make sure they exist when used
        public static class Emote
        {
            public const ulong SELF = 846789266041470977UL;
            public const ulong OWNER = 356017256481685506UL;

            public static class Vote
            {
                public const ulong UP = 854440379948466176UL;
                public const ulong DOWN = 854440380031565834UL;
                public const ulong NONE = 854441334852943903UL;
            }
        }
    }
}