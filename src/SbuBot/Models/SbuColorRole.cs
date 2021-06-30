using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuColorRole : SbuEntityBase, ISbuDiscordEntity, ISbuOwnedEntity, ISbuGuildEntity
    {
        public const int MAX_NAME_LENGTH = 100;
        public Snowflake Id { get; }
        public Snowflake? OwnerId { get; set; }
        public Snowflake GuildId { get; }

        // nav properties
        [NotLogged]
        public SbuMember? Owner { get; }

        [NotLogged]
        public SbuGuild? Guild { get; }

        public SbuColorRole(Snowflake id, Snowflake? ownerId, Snowflake guildId)
        {
            Id = id;
            OwnerId = ownerId;
            GuildId = guildId;
        }

        public SbuColorRole(IRole role, Snowflake ownerId)
        {
            Id = role.Id;
            OwnerId = ownerId;
            GuildId = role.GuildId;
        }

#region EFCore

#nullable disable

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuColorRole>
        {
            public void Configure(EntityTypeBuilder<SbuColorRole> builder)
            {
                builder.HasKey(cr => new { cr.Id, cr.GuildId });

                builder.Property(cr => cr.OwnerId);
                builder.Property(cr => cr.GuildId);

                builder.HasOne(cr => cr.Owner)
                    .WithOne(m => m.ColorRole)
                    .HasForeignKey<SbuColorRole>(cr => new { cr.OwnerId, cr.GuildId })
                    .HasPrincipalKey<SbuMember>(m => new { m.Id, m.GuildId })
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasOne(cr => cr.Guild)
                    .WithMany(m => m.ColorRoles)
                    .HasForeignKey(cr => cr.GuildId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

#endregion
    }
}