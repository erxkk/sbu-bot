using System;
using System.Diagnostics.CodeAnalysis;

using Destructurama.Attributed;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuReminder : ISbuOwnedGuildEntity
    {
        public const int MAX_MESSAGE_LENGTH = 1024;

        [NotNull]
        public Snowflake? OwnerId { get; set; }

        public Snowflake GuildId { get; }
        public Snowflake ChannelId { get; }
        public Snowflake MessageId { get; }
        public string? Message { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset DueAt { get; set; }

        // nav properties
        [NotLogged]
        public SbuMember? Owner { get; } = null!;

        [NotLogged]
        public SbuGuild? Guild { get; } = null!;

        public SbuReminder(
            DiscordGuildCommandContext context,
            Snowflake ownerId,
            Snowflake guildId,
            string? message,
            DateTimeOffset dueAt
        ) : this(
            ownerId,
            guildId,
            context.ChannelId,
            context.Message.Id,
            message,
            DateTimeOffset.Now,
            dueAt
        ) { }

        public string GetJumpUrl() => Discord.MessageJumpLink(GuildId, ChannelId, MessageId);

        public string GetFormattedId() => MessageId.RawValue.ToString("X");

#region EFCore

        public SbuReminder(
            Snowflake? ownerId,
            Snowflake guildId,
            Snowflake channelId,
            Snowflake messageId,
            string? message,
            DateTimeOffset createdAt,
            DateTimeOffset dueAt
        )
        {
            OwnerId = ownerId;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            Message = message;
            CreatedAt = createdAt;
            DueAt = dueAt;
        }

#nullable disable
        public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuReminder>
        {
            public void Configure(EntityTypeBuilder<SbuReminder> builder)
            {
                builder.HasKey(t => t.MessageId);
                builder.HasIndex(t => t.OwnerId);
                builder.HasIndex(t => t.GuildId);

                builder.Property(t => t.OwnerId);
                builder.Property(t => t.GuildId);
                builder.Property(t => t.ChannelId);
                builder.Property(t => t.MessageId);
                builder.Property(t => t.Message).HasMaxLength(SbuReminder.MAX_MESSAGE_LENGTH);
                builder.Property(t => t.CreatedAt);
                builder.Property(t => t.DueAt);

                builder.HasOne(r => r.Owner)
                    .WithMany(m => m.Reminders)
                    .HasForeignKey(r => new { r.OwnerId, r.GuildId })
                    .HasPrincipalKey(m => new { m.Id, m.GuildId })
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(r => r.Guild)
                    .WithMany(g => g.Reminders)
                    .HasForeignKey(r => r.GuildId)
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
#nullable enable

#endregion
    }
}
