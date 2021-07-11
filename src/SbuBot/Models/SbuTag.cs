using System;
using System.Linq;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuTag : ISbuOwnedGuildEntity
    {
        public const int MIN_NAME_LENGTH = 3;
        public const int MAX_NAME_LENGTH = 128;
        public const int MAX_CONTENT_LENGTH = 2048;

        public Snowflake? OwnerId { get; set; }
        public Snowflake GuildId { get; }
        public string Name { get; }
        public string Content { get; set; }

        // nav properties
        [NotLogged]
        public SbuMember? Owner { get; }

        [NotLogged]
        public SbuGuild? Guild { get; }

        public SbuTag(Snowflake ownerId, Snowflake guildId, string name, string content) : this(
            (Snowflake?) ownerId,
            guildId,
            name,
            content
        ) { }

#region EFCore

        internal SbuTag(Snowflake? ownerId, Snowflake guildId, string name, string content)
        {
            OwnerId = ownerId;
            GuildId = guildId;
            Name = name;
            Content = content;
        }

#nullable disable
        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuTag>
        {
            public void Configure(EntityTypeBuilder<SbuTag> builder)
            {
                builder.HasKey(t => new { t.Name, t.GuildId });
                builder.HasIndex(t => t.Name);
                builder.HasIndex(t => t.OwnerId);
                builder.HasIndex(t => t.GuildId);

                builder.Property(t => t.OwnerId);
                builder.Property(t => t.GuildId);
                builder.Property(t => t.Name).HasMaxLength(MAX_NAME_LENGTH);
                builder.Property(t => t.Content).HasMaxLength(MAX_CONTENT_LENGTH).IsRequired();

                builder.HasOne(t => t.Owner)
                    .WithMany(m => m.Tags)
                    .HasForeignKey(t => new { t.OwnerId, t.GuildId })
                    .HasPrincipalKey(m => new { m.Id, m.GuildId })
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasOne(t => t.Guild)
                    .WithMany(m => m.Tags)
                    .HasForeignKey(t => t.GuildId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

#endregion

#region Validation

        public enum ValidNameType
        {
            Valid,
            TooShort,
            TooLong,
            Reserved,
        }

        public static ValidNameType IsValidTagName(string name)
        {
            return name.Length switch
            {
                < MIN_NAME_LENGTH => ValidNameType.TooShort,
                > MAX_NAME_LENGTH => ValidNameType.TooLong,
                _ => SbuGlobals.RESERVED_KEYWORDS.Any(rn => rn.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ? ValidNameType.Reserved
                    : ValidNameType.Valid,
            };
        }

#endregion
    }
}