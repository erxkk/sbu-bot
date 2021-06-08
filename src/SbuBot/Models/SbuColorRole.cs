using System;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuColorRole : SbuEntityBase, ISbuDiscordEntity, ISbuOwnedEntity
    {
        public const int MAX_NAME_LENGTH = 100;

        public Snowflake DiscordId { get; }
        public Snowflake? OwnerId { get; set; }

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuMember? Owner { get; set; }

        public SbuColorRole(Snowflake discordId, Snowflake ownerId)
        {
            DiscordId = discordId;
            OwnerId = ownerId;
        }

        public SbuColorRole(IRole role, Snowflake ownerId)
        {
            DiscordId = role.Id;
            OwnerId = ownerId;
        }

#region EFCore

        internal SbuColorRole(Guid id, Snowflake discordId, Snowflake? ownerId) : base(id)
        {
            DiscordId = discordId;
            OwnerId = ownerId;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuColorRole>
        {
            public void Configure(EntityTypeBuilder<SbuColorRole> builder)
            {
                builder.HasKey(cr => cr.Id);
                builder.HasIndex(cr => cr.DiscordId).IsUnique();
                builder.HasIndex(cr => cr.OwnerId).IsUnique();

                builder.HasOne(cr => cr.Owner)
                    .WithOne(m => m.ColorRole)
                    .HasForeignKey<SbuColorRole>(cr => cr.OwnerId)
                    .HasPrincipalKey<SbuMember>(m => m.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);
            }
        }

#endregion
    }
}