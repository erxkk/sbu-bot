using Disqord;

namespace SbuBot.Models
{
    public interface ISbuGuildEntity
    {
        public Snowflake GuildId { get; }
        public SbuGuild? Guild { get; }
    }
}
