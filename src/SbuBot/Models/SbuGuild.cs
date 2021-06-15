using System;
using System.Collections.Generic;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuGuild : SbuEntityBase, ISbuDiscordEntity
    {
        public Snowflake DiscordId { get; set; }

        // nav properties

        [HideOnSerialize, NotLogged]
        public List<SbuMember> Members { get; }

        [HideOnSerialize, NotLogged]
        public List<SbuColorRole> ColorRoles { get; }

        [HideOnSerialize, NotLogged]
        public List<SbuReminder> Reminders { get; }

        [HideOnSerialize, NotLogged]
        public List<SbuTag> Tags { get; }

        public SbuGuild(Snowflake discordId) => DiscordId = discordId;
        public SbuGuild(IGuild guild) => DiscordId = guild.Id;

#region EFCore

        internal SbuGuild(Guid id, Snowflake discordId) : base(id)
        {
            DiscordId = discordId;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuGuild>
        {
            public void Configure(EntityTypeBuilder<SbuGuild> builder)
            {
                builder.HasKey(g => g.Id);
                builder.HasIndex(g => g.DiscordId).IsUnique();

                builder.Property(g => g.DiscordId);

                builder.HasMany(g => g.Members)
                    .WithOne(m => m.Guild)
                    .HasForeignKey(m => m.GuildId)
                    .HasPrincipalKey(g => g.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(g => g.ColorRoles)
                    .WithOne(cr => cr.Guild)
                    .HasForeignKey(cr => cr.OwnerId)
                    .HasPrincipalKey(g => g.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(g => g.Tags)
                    .WithOne(t => t.Guild)
                    .HasForeignKey(t => t.OwnerId)
                    .HasPrincipalKey(g => g.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(g => g.Reminders)
                    .WithOne(r => r.Guild)
                    .HasForeignKey(r => r.OwnerId)
                    .HasPrincipalKey(g => g.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);
            }
        }

#endregion
    }
}