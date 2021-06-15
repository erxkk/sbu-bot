using System;
using System.Linq;

using Destructurama.Attributed;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuTag : SbuEntityBase, ISbuOwnedEntity, ISbuGuildEntity
    {
        public const int MIN_NAME_LENGTH = 3;
        public const int MAX_NAME_LENGTH = 128;
        public const int MAX_CONTENT_LENGTH = 2048;

        public Guid? OwnerId { get; set; }
        public Guid? GuildId { get; set; }
        public string Name { get; }
        public string Content { get; set; }

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuMember? Owner { get; }

        [HideOnSerialize, NotLogged]
        public SbuGuild? Guild { get; }

        public SbuTag(Guid ownerId, Guid guildId, string name, string content)
        {
            OwnerId = ownerId;
            GuildId = guildId;
            Name = name;
            Content = content;
        }

#region EFCore

        internal SbuTag(Guid id, Guid? ownerId, Guid guildId, string name, string content) : base(id)
        {
            OwnerId = ownerId;
            GuildId = guildId;
            Name = name;
            Content = content;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuTag>
        {
            public void Configure(EntityTypeBuilder<SbuTag> builder)
            {
                builder.HasKey(t => t.Id);
                builder.HasIndex(t => t.OwnerId);
                builder.HasIndex(t => new { t.Name, t.GuildId }).IsUnique();

                builder.Property(t => t.OwnerId);
                builder.Property(t => t.GuildId);
                builder.Property(t => t.Name).HasMaxLength(SbuTag.MAX_NAME_LENGTH);
                builder.Property(t => t.Content).HasMaxLength(SbuTag.MAX_CONTENT_LENGTH).IsRequired();

                builder.HasOne(t => t.Owner)
                    .WithMany(m => m.Tags)
                    .HasForeignKey(t => t.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasOne(t => t.Guild)
                    .WithMany(m => m.Tags)
                    .HasForeignKey(t => t.GuildId)
                    .HasPrincipalKey(m => m.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);
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
                < SbuTag.MIN_NAME_LENGTH => ValidNameType.TooShort,
                > SbuTag.MAX_NAME_LENGTH => ValidNameType.TooLong,
                _ => SbuGlobals.RESERVED_KEYWORDS.Any(rn => rn.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ? ValidNameType.Reserved
                    : ValidNameType.Valid,
            };
        }

#endregion
    }
}