using System.Collections.Generic;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuGuild : SbuEntityBase, ISbuDiscordEntity
    {
        public Snowflake Id { get; }

        // nav properties
        [NotLogged]
        public List<SbuMember> Members { get; } = new();

        [NotLogged]
        public List<SbuColorRole> ColorRoles { get; } = new();

        [NotLogged]
        public List<SbuReminder> Reminders { get; } = new();

        [NotLogged]
        public List<SbuTag> Tags { get; } = new();

        public SbuGuild(Snowflake id) => Id = id;
        public SbuGuild(IGuild guild) => Id = guild.Id;

#region EFCore

#nullable disable

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuGuild>
        {
            public void Configure(EntityTypeBuilder<SbuGuild> builder)
            {
                builder.HasKey(g => g.Id);

                builder.HasMany(g => g.Members)
                    .WithOne(m => m.Guild)
                    .HasForeignKey(m => m.GuildId)
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(g => g.ColorRoles)
                    .WithOne(cr => cr.Guild)
                    .HasForeignKey(cr => cr.GuildId)
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(g => g.Tags)
                    .WithOne(t => t.Guild)
                    .HasForeignKey(t => t.GuildId)
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasMany(g => g.Reminders)
                    .WithOne(r => r.Guild)
                    .HasForeignKey(r => r.GuildId)
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

#endregion
    }
}