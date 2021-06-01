using Disqord;

namespace SbuBot
{
    public static class SbuBotGlobals
    {
        public static readonly Snowflake OWNER_ID = 356017256481685506ul;
        public const string DEFAULT_PREFIX = "sbu";

        public static readonly string[] RESERVED_NAMES =
        {
            "help", "cancel", "new", "create", "make", "delete", "remove", "list", "show", "claim", "gift", "all",
            "none", "mine",
        };

        public static class Guild
        {
            public static readonly Snowflake OWNER_ID = 356017256481685506ul;
            public static readonly Snowflake ID = 681285811945340959ul; // original: 732210852849123418ul;
            public const string NAME = "Siege Bad Uninstall";

            public static class Roles
            {
                public static readonly Snowflake BOOSTER = 732232124714713229ul;

                public static class Separators
                {
                    public static readonly Snowflake PERM = 776790664436383794ul;
                    public static readonly Snowflake MENTION = 732238126998880267ul;
                    public static readonly Snowflake COLOR = 753986150724534404ul;
                }

                public static class Permissions
                {
                    public static readonly Snowflake ADMIN = 741033629613948961ul;
                    public static readonly Snowflake BOT = 755086820471210086ul; // test server role
                    public static readonly Snowflake MUTED = 772062437490294784ul;
                    public static readonly Snowflake NO_SERIOUS_CHANNEL = 776789839793618984ul;
                    public static readonly Snowflake THE_SENATE_SUBMISSION = 775428028771598346ul;
                    public static readonly Snowflake SHIT_SBU_SAYS_SUBMISSION = 759189942730883083ul;
                    public static readonly Snowflake ANNOUNCEMENT_SUBMISSION = 773225941551153182ul;
                    public static readonly Snowflake SERIOUS_CHANNEL_GUARDIAN = 789078423385407488ul;
                    public static readonly Snowflake PIN_BRIGADE = 817448908095488000ul;
                }
            }
        }

        public static class Emotes
        {
            public static readonly Snowflake GUILD_ID = 846789266041470977ul;
        }
    }
}