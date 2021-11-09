using System.Collections.Generic;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuRole : ISbuDiscordEntity, ISbuGuildEntity
    {
        public Snowflake Id { get; }
        public Snowflake GuildId { get; }
        public string? Description { get; }

        // nav properties
        [NotLogged]

        public SbuGuild? Guild { get; } = null!;

        public SbuRole(IRole role, string? description = null) : this(role.Id, role.GuildId, description) { }

#region EFCore

        public SbuRole(Snowflake id, Snowflake guildId, string? description)
        {
            Id = id;
            GuildId = guildId;
            Description = description;
        }

#nullable disable

        public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuRole>
        {
            public void Configure(EntityTypeBuilder<SbuRole> builder)
            {
                builder.HasKey(r => r.Id);

                builder.Property(r => r.GuildId);
                builder.Property(r => r.Description).HasMaxLength(1024);

                builder.HasOne(g => g.Guild)
                    .WithMany(r => r.Roles)
                    .HasForeignKey(r => r.GuildId)
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
#nullable enable

#endregion
    }
}