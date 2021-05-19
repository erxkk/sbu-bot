using Disqord;

namespace SbuBot
{
    public static class SbuBotGlobals
    {
        public static readonly Snowflake GUILD_ID = 732210852849123418ul;
        public const string GUILD_NAME = "Siege Bad Uninstall";

        public static class Roles
        {
            public static readonly Snowflake ADMIN = 741033629613948961ul;

            // TODO: add after bot joined
            public static readonly Snowflake BOT = 0ul;
            public static readonly Snowflake MUTED = 772062437490294784ul;
            public static readonly Snowflake NO_SERIOUS_CHANNEL = 776789839793618984ul;
            public static readonly Snowflake THE_SENATE_SUBMISSION = 775428028771598346ul;
            public static readonly Snowflake SHIT_SBU_SAYS_SUBMISSION = 759189942730883083ul;
            public static readonly Snowflake ANNOUNCEMENT_SUBMISSION = 773225941551153182ul;
            public static readonly Snowflake SERIOUS_CHANNEL_GUARDIAN = 789078423385407488ul;
            public static readonly Snowflake BOOSTER = 732232124714713229ul;
            public static readonly Snowflake PIN_BRIGADE = 817448908095488000ul;
        }
    }
}