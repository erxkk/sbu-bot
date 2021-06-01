using System;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuTag : SbuEntityBase, ISbuOwnedEntity
    {
        public const int MIN_NAME_LENGTH = 3;

        public Snowflake? OwnerId { get; set; }
        public string Name { get; }
        public string Content { get; set; }

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuMember? Owner { get; }

        public SbuTag(Snowflake ownerId, string name, string content)
        {
            OwnerId = ownerId;
            Name = name;
            Content = content;
        }

#region EFCore

        internal SbuTag(Guid id, Snowflake? ownerId, string name, string content) : base(id)
        {
            OwnerId = ownerId;
            Name = name;
            Content = content;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuTag>
        {
            public void Configure(EntityTypeBuilder<SbuTag> builder)
            {
                builder.HasKey(t => t.Id);
                builder.HasIndex(t => t.OwnerId);
                builder.HasIndex(t => t.Name).IsUnique();

                builder.Property(t => t.OwnerId);
                builder.Property(t => t.Name).HasMaxLength(128);
                builder.Property(t => t.Content).HasMaxLength(2048).IsRequired();

                builder.HasOne(t => t.Owner)
                    .WithMany(m => m.Tags)
                    .HasForeignKey(t => t.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);
            }
        }

#endregion
    }
}