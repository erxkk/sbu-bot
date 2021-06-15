using Disqord;

namespace SbuBot.Models
{
    public interface ISbuDiscordEntity : ISbuEntity
    {
        public Snowflake DiscordId { get; set; }
    }
}