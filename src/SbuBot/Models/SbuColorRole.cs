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
        public Snowflake GuildId { get; set; }

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuMember? Owner { get; set; }

        [HideOnSerialize, NotLogged]
        public SbuGuild? Guild { get; }

        public SbuColorRole(Snowflake id, Snowflake? ownerId, Snowflake guildId)
        {
            Id = id;
            OwnerId = ownerId;
            GuildId = guildId;
        }

        public SbuColorRole(IRole role, Snowflake ownerId, Snowflake guildId)
        {
            Id = role.Id;
            OwnerId = ownerId;
            GuildId = guildId;
        }

#region EFCore

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuColorRole>
        {
            public void Configure(EntityTypeBuilder<SbuColorRole> builder)
            {
                builder.HasKey(cr => cr.Id);
                builder.HasIndex(cr => new { cr.OwnerId, cr.GuildId }).IsUnique();

                builder.Property(cr => cr.OwnerId);
                builder.Property(cr => cr.GuildId);

                builder.HasOne(cr => cr.Owner)
                    .WithOne(m => m.ColorRole)
                    .HasForeignKey<SbuColorRole>(cr => cr.OwnerId)
                    .HasPrincipalKey<SbuMember>(m => m.Id)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasOne(cr => cr.Guild)
                    .WithMany(m => m.ColorRoles)
                    .HasForeignKey(cr => cr.GuildId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.SetNull);
            }
        }

#endregion
    }
}