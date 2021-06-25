using System.Collections.Generic;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuMember : SbuEntityBase, ISbuDiscordEntity, ISbuGuildEntity
    {
        public Snowflake Id { get; }
        public Snowflake GuildId { get; }

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuGuild Guild { get; }

        [HideOnSerialize, NotLogged]
        public SbuColorRole? ColorRole { get; set; }

        [HideOnSerialize, NotLogged]
        public List<SbuTag> Tags { get; } = new();

        [HideOnSerialize, NotLogged]
        public List<SbuReminder> Reminders { get; } = new();

        public SbuMember(Snowflake id, Snowflake guildId)
        {
            Id = id;
            GuildId = guildId;
        }

        public SbuMember(IMember member, Snowflake guildId)
        {
            Id = member.Id;
            GuildId = guildId;
        }

#region EFCore

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuMember>
        {
            public void Configure(EntityTypeBuilder<SbuMember> builder)
            {
                builder.HasKey(m => m.Id);

                builder.Property(m => m.GuildId);

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