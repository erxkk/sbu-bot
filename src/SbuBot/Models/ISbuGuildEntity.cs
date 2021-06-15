using System;

namespace SbuBot.Models
{
    public interface ISbuGuildEntity
    {
        public Guid? GuildId { get; set; }
        public SbuGuild? Guild { get; }
    }
}