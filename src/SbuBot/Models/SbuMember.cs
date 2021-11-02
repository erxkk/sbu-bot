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

        public SbuMember(IMember member) : this(member.Id, member.GuildId) { }

#region EFCore

        public SbuMember(Snowflake id, Snowflake guildId)
        {
            Id = id;
            GuildId = guildId;
        }

#nullable disable
        public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuMember>
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
                    .HasForeignKey(t => new { t.OwnerId, t.GuildId })
                    .HasPrincipalKey(m => new { m.Id, m.GuildId })
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(m => m.Reminders)
                    .WithOne(r => r.Owner)
                    .HasForeignKey(r => new { r.OwnerId, r.GuildId })
                    .HasPrincipalKey(m => new { m.Id, m.GuildId })
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
#nullable enable

#endregion
    }
}