namespace SbuBot
{
    public static class SbuBotGlobals
    {
        public const char DESCRIPTOR_SEPARATOR = '|';

        public static readonly string[] RESERVED_KEYWORDS =
        {
            "help", "cancel", "create", "make", "delete", "remove", "list", "claim", "transfer", "all", "none", "mine",
            "reserved", "abort",
        };

        public const string DEFAULT_PREFIX = "sbu";

        public static class Bot
        {
            public const ulong ID = 849571821930283018ul;
            public const ulong OWNER_ID = 356017256481685506ul;
            public const string NAME = "Allah 2";
        }

        public static class Guild
        {
            public const ulong ID = 732210852849123418ul;
            public const ulong OWNER_ID = 356017256481685506ul;
            public const string NAME = "Siege Bad Uninstall";
        }

        public static class Roles
        {
            public const ulong BOOSTER = 732232124714713229ul;

            // empty roles that are above the roles they annotate
            public static class Categories
            {
                public const ulong PERM = 776790664436383794ul;
                public const ulong MENTION = 732238126998880267ul;
                public const ulong COLOR = 753986150724534404ul;
            }

            public const ulong ADMIN = 741033629613948961ul;
            public const ulong BOT = 851039575476273193ul;
            public const ulong MUTED = 772062437490294784ul;
            public const ulong NO_SERIOUS_CHANNEL = 776789839793618984ul;
            public const ulong THE_SENATE_SUBMISSION = 775428028771598346ul;
            public const ulong SHIT_SBU_SAYS_SUBMISSION = 759189942730883083ul;
            public const ulong ANNOUNCEMENT_SUBMISSION = 773225941551153182ul;
            public const ulong SERIOUS_CHANNEL_GUARDIAN = 789078423385407488ul;
            public const ulong PIN_BRIGADE = 817448908095488000ul;
        }

        public static class Channels
        {
            // actual category channels
            public static class Categories
            {
                public const ulong NORMAL = 732210852849123419ul;
                public const ulong BASED = 760469720741969950ul;
                public const ulong SERIOUS = 732215815537164309ul;
                public const ulong VOICE = 732210852849123420ul;
            }

            public const ulong HELP = 732210852849123421ul;
            public const ulong ANNOUNCEMENTS = 732231139233759324ul;
            public const ulong SENATE = 775427903206457365ul;
            public const ulong PIN_ARCHIVE = 775826206410407937ul;
            public const ulong SHIT_SBU_SAYS = 759153822806310912ul;
            public const ulong CRUMPET_SERVER = 790542075346944020ul;
        }

        // saved on different guild to make sure they exist when used
        public static class Emotes
        {
            public const ulong ID = 846789266041470977ul;
            public const ulong OWNER_ID = 356017256481685506ul;
        }
    }
}