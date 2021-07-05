using System.Collections.Generic;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuMember : ISbuDiscordEntity, ISbuGuildEntity
    {
        public Snowflake Id { get; }
        public Snowflake GuildId { get; }

        // nav properties
        [NotLogged]
        public SbuGuild? Guild { get; }

        [NotLogged]
        public SbuColorRole? ColorRole { get; set; }

        [NotLogged]
        public List<SbuTag> Tags { get; } = new();

        [NotLogged]
        public List<SbuReminder> Reminders { get; } = new();

        public SbuMember(Snowflake id, Snowflake guildId)
        {
            Id = id;
            GuildId = guildId;
        }

        public SbuMember(IMember member)
        {
            Id = member.Id;
            GuildId = member.GuildId;
        }

#region EFCore

#nullable disable
        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuMember>
        {
            public void Configure(EntityTypeBuilder<SbuMember> builder)
            {
                builder.HasKey(m => new { m.Id, m.GuildId });

                builder.Property(m => m.GuildId);

                builder.HasOne(m => m.Guild)
                    .WithMany(g => g.Members)
                    .HasForeignKey(g => g.GuildId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(m => m.ColorRole)
                    .WithOne(cr => cr.Owner)
                    .HasForeignKey<SbuColorRole>(cr => new { cr.OwnerId, cr.GuildId })
                    .HasPrincipalKey<SbuMember>(m => new { m.Id, m.GuildId })
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