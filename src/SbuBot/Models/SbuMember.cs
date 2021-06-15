using System;
using System.Collections.Generic;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuMember : SbuEntityBase, ISbuDiscordEntity, ISbuGuildEntity
    {
        public Snowflake DiscordId { get; set; }
        public Guid? GuildId { get; set; }

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuGuild? OwnedGuild { get; }

        [HideOnSerialize, NotLogged]
        public SbuGuild Guild { get; }

        [HideOnSerialize, NotLogged]
        public SbuColorRole? ColorRole { get; }

        [HideOnSerialize, NotLogged]
        public List<SbuTag> Tags { get; } = new();

        [HideOnSerialize, NotLogged]
        public List<SbuReminder> Reminders { get; } = new();

        public SbuMember(Snowflake discordId, Guid guildId)
        {
            DiscordId = discordId;
            GuildId = guildId;
        }

        public SbuMember(IMember member, Guid guildId)
        {
            DiscordId = member.Id;
            GuildId = guildId;
        }

#region EFCore

        internal SbuMember(Guid id, Snowflake discordId, Guid? guildId) : base(id) => DiscordId = discordId;

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuMember>
        {
            public void Configure(EntityTypeBuilder<SbuMember> builder)
            {
                builder.HasKey(m => m.Id);
                builder.HasIndex(m => m.DiscordId).IsUnique();

                builder.Property(m => m.DiscordId);

                builder.HasOne(m => m.Guild)
                    .WithMany(g => g.Members)
                    .HasForeignKey(g => g.GuildId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasOne(m => m.ColorRole)
                    .WithOne(cr => cr.Owner)
                    .HasForeignKey<SbuColorRole>(cr => cr.OwnerId)
                    .HasPrincipalKey<SbuMember>(m => m.Id)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(m => m.Tags)
                    .WithOne(t => t.Owner)
                    .HasForeignKey(t => t.OwnerId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(m => m.Reminders)
                    .WithOne(r => r.Owner)
                    .HasForeignKey(r => r.OwnerId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

#endregion
    }
}