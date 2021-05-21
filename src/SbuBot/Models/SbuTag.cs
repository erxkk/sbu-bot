using System;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed record SbuTag : SbuEntity
    {
        public Snowflake OwnerId { get; }
        public string Name { get; }
        public string Value { get; set; }

        // nav properties
        public SbuMember Owner { get; }

        // new
        public SbuTag(Snowflake ownerId, string name, string value)
        {
            OwnerId = ownerId;
            Name = name;
            Value = value;
        }

        // ef core
        internal SbuTag(Guid id, Snowflake ownerId, string name, string value) : base(id)
        {
            OwnerId = ownerId;
            Name = name;
            Value = value;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuTag>
        {
            public void Configure(EntityTypeBuilder<SbuTag> builder)
            {
                builder.HasKey(t => t.Id);
                builder.HasIndex(t => t.OwnerId);

                builder.HasIndex(t => t.Name)
                    .IsUnique();

                builder.Property(t => t.OwnerId);

                builder.Property(t => t.Name)
                    .HasMaxLength(128);

                builder.Property(t => t.Value)
                    .IsRequired()
                    .HasMaxLength(2048);

                builder.HasOne(t => t.Owner)
                    .WithMany(m => m.Tags)
                    .HasForeignKey(t => t.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId);
            }
        }
    }
}