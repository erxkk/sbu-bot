using System;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuColorRole : SbuEntityBase, ISbuDiscordEntity, ISbuOwnedEntity, ISbuGuildEntity
    {
        public const int MAX_NAME_LENGTH = 100;

        public Snowflake DiscordId { get; set; }
        public Guid? OwnerId { get; set; }
        public Guid? GuildId { get; set; }
        public string Name { get; set; }
        public Color? Color { get; set; }

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuMember? Owner { get; set; }

        [HideOnSerialize, NotLogged]
        public SbuGuild? Guild { get; }

        public SbuColorRole(Snowflake discordId, Guid ownerId, Guid guildId, string name, Color? color)
        {
            DiscordId = discordId;
            OwnerId = ownerId;
            GuildId = guildId;
            Name = name;
            Color = color;
        }

        public SbuColorRole(IRole role, Guid ownerId, Guid guildId)
        {
            DiscordId = role.Id;
            OwnerId = ownerId;
            GuildId = guildId;
            Name = role.Name;
            Color = role.Color;
        }

#region EFCore

        internal SbuColorRole(Guid id, Snowflake discordId, Guid? ownerId, Guid? guildId) : base(id)
        {
            DiscordId = discordId;
            OwnerId = ownerId;
            GuildId = guildId;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuColorRole>
        {
            public void Configure(EntityTypeBuilder<SbuColorRole> builder)
            {
                builder.HasKey(cr => cr.Id);
                builder.HasIndex(cr => cr.DiscordId).IsUnique();
                builder.HasIndex(cr => cr.OwnerId).IsUnique();

                builder.Property(cr => cr.Name).HasMaxLength(SbuColorRole.MAX_NAME_LENGTH);
                builder.Property(cr => cr.Color);

                builder.HasOne(cr => cr.Owner)
                    .WithOne(m => m.ColorRole)
                    .HasForeignKey<SbuColorRole>(cr => cr.OwnerId)
                    .HasPrincipalKey<SbuMember>(m => m.Id)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasOne(cr => cr.Guild)
                    .WithMany(m => m.ColorRoles)
                    .HasForeignKey(cr => cr.OwnerId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.SetNull);
            }
        }

#endregion
    }
}