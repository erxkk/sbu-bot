using System;
using System.Collections.Generic;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed record SbuMember : SbuEntity
    {
        public Snowflake DiscordId { get; set; }

        // nav properties
        public SbuColorRole? ColorRole { get; }
        public List<SbuTag> Tags { get; } = new();
        public List<SbuNicknameLog> Nicknames { get; } = new();
        public List<SbuReminder> Reminders { get; } = new();

        // new
        public SbuMember(Snowflake id) => DiscordId = id;

        // ef core
        public SbuMember(Guid id, Snowflake discordId) : base(id) => DiscordId = discordId;

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuMember>
        {
            public void Configure(EntityTypeBuilder<SbuMember> builder)
            {
                builder.HasKey(m => m.Id);

                builder.HasIndex(m => m.DiscordId)
                    .IsUnique();

                builder.HasOne(m => m.ColorRole)
                    .WithOne(cr => cr.Owner)
                    .HasForeignKey<SbuColorRole>(cr => cr.OwnerId)
                    .HasPrincipalKey<SbuMember>(m => m.DiscordId);

                builder.HasMany(m => m.Tags)
                    .WithOne(t => t.Owner)
                    .HasForeignKey(t => t.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId);

                builder.HasMany(m => m.Nicknames)
                    .WithOne(nl => nl.Owner)
                    .HasForeignKey(nl => nl.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId);

                builder.HasMany(m => m.Reminders)
                    .WithOne(r => r.Owner)
                    .HasForeignKey(r => r.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId);
            }
        }
    }
}