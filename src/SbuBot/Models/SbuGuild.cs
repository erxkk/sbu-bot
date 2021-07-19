using System;
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

        public SbuGuild(IGuild guild) : this(guild.Id, (SbuGuildConfig) 255) { }

#region EFCore

        internal SbuGuild(Snowflake id, SbuGuildConfig config)
        {
            Id = id;
            Config = config;
        }

#nullable disable

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuGuild>
        {
            public void Configure(EntityTypeBuilder<SbuGuild> builder)
            {
                builder.HasKey(g => g.Id);

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

    [Flags]
    public enum SbuGuildConfig : byte
    {
        Fun,
        Respond,
    }
}