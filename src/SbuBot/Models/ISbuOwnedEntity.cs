using Disqord;

namespace SbuBot.Models
{
    public interface ISbuOwnedEntity : ISbuEntity
    {
        public Snowflake? OwnerId { get; set; }
    }
}