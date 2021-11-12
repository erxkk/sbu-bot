using Disqord;

namespace SbuBot.Models
{
    public interface ISbuOwnedEntity
    {
        public Snowflake? OwnerId { get; set; }
        public SbuMember? Owner { get; }
    }
}
