using System;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed record SbuColorRole : SbuEntity
    {
        public Snowflake DiscordId { get; }
        public Snowflake OwnerId { get; }
        public Color Color { get; set; }

        public string Name { get; set; }

        // nav properties
        public SbuMember? Owner { get; }

        // new
        public SbuColorRole(Snowflake discordId, Snowflake ownerId, Color color, string name)
        {
            DiscordId = discordId;
            OwnerId = ownerId;
            Color = color;
            Name = name;
        }

        // ef core
        internal SbuColorRole(Guid id, Snowflake discordId, Snowflake ownerId, Color color, string name) : base(id)
        {
            DiscordId = discordId;
            OwnerId = ownerId;
            Color = color;
            Name = name;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuColorRole>
        {
            public void Configure(EntityTypeBuilder<SbuColorRole> builder)
            {
                builder.HasKey(cr => cr.Id);

                builder.HasIndex(cr => cr.DiscordId)
                    .IsUnique();

                builder.HasIndex(cr => cr.OwnerId)
                    .IsUnique();

                builder.Property(cr => cr.Color)
                    .IsRequired();

                builder.Property(cr => cr.Name)
                    .HasMaxLength(100);

                builder.HasOne(cr => cr.Owner)
                    .WithOne(m => m.ColorRole)
                    .HasForeignKey<SbuColorRole>(cr => cr.OwnerId)
                    .HasPrincipalKey<SbuMember>(m => m.DiscordId);
            }
        }
    }
}