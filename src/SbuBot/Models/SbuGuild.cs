using System.Collections.Generic;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuGuild : ISbuDiscordEntity
    {
        public Snowflake Id { get; }
        public Snowflake? ArchiveId { get; set; }
        public Snowflake? ColorRoleTopId { get; set; }
        public Snowflake? ColorRoleBottomId { get; set; }
        public SbuGuildConfig Config { get; set; }

        // nav properties
        [NotLogged]
        public List<SbuMember> Members { get; } = new();

        [NotLogged]
        public List<SbuColorRole> ColorRoles { get; } = new();

        [NotLogged]
        public List<SbuTag> Tags { get; } = new();

        [NotLogged]
        public List<SbuAutoResponse> AutoResponses { get; } = new();

        [NotLogged]
        public List<SbuReminder> Reminders { get; } = new();

        public SbuGuild(
            IGuild guild,
            Snowflake? archiveId = null,
            Snowflake? colorRoleTopId = null,
            Snowflake? colorRoleBottomId = null
        ) : this(guild.Id, archiveId, colorRoleTopId, colorRoleBottomId, (SbuGuildConfig)255) { }

#region EFCore

        public SbuGuild(
            Snowflake id,
            Snowflake? archiveId,
            Snowflake? colorRoleTopId,
            Snowflake? colorRoleBottomId,
            SbuGuildConfig config
        )
        {
            Id = id;
            Config = config;
            ArchiveId = archiveId;
            ColorRoleTopId = colorRoleTopId;
            ColorRoleBottomId = colorRoleBottomId;
        }

#nullable disable

        public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuGuild>
        {
            public void Configure(EntityTypeBuilder<SbuGuild> builder)
            {
                builder.HasKey(g => g.Id);

                builder.Property(g => g.ArchiveId);
                builder.Property(g => g.ColorRoleTopId);
                builder.Property(g => g.ColorRoleBottomId);
                builder.Property(g => g.Config);

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

                builder.HasMany(ar => ar.AutoResponses)
                    .WithOne(g => g.Guild)
                    .HasForeignKey(ar => ar.GuildId)
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