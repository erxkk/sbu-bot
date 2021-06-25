using System;
using System.Diagnostics.CodeAnalysis;

using Destructurama.Attributed;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuReminder : SbuEntityBase, ISbuEntity, ISbuOwnedEntity, ISbuGuildEntity
    {
        public const int MAX_MESSAGE_LENGTH = 1024;
        public Guid Id { get; }

        [NotNull]
        public Snowflake? OwnerId { get; set; }
        public Snowflake GuildId { get; }
        public Snowflake ChannelId { get; }
        public Snowflake MessageId { get; }
        public string? Message { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset DueAt { get; set; }

        [HideOnSerialize, NotLogged]
        public string JumpUrl => string.Format(
            "https://discord.com/channels/{0}/{1}/{2}",
            GuildId,
            ChannelId,
            MessageId
        );

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuMember? Owner { get; }

        [HideOnSerialize, NotLogged]
        public SbuGuild? Guild { get; }

        public SbuReminder(
            Snowflake ownerId,
            Snowflake guildId,
            Snowflake channelId,
            Snowflake messageId,
            string? message,
            DateTimeOffset createdAt,
            DateTimeOffset dueAt
        )
        {
            Id = Guid.NewGuid();
            OwnerId = ownerId;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            Message = message;
            CreatedAt = createdAt;
            DueAt = dueAt;
        }

        public SbuReminder(
            DiscordGuildCommandContext context,
            Snowflake ownerId,
            Snowflake guildId,
            string? message,
            DateTimeOffset dueAt
        )
        {
            Id = Guid.NewGuid();
            OwnerId = ownerId;
            GuildId = guildId;
            ChannelId = context.ChannelId;
            MessageId = context.Message.Id;
            Message = message;
            CreatedAt = DateTimeOffset.Now;
            DueAt = dueAt;
        }

#region EFCore

        internal SbuReminder(
            Guid id,
            Snowflake? ownerId,
            Snowflake guildId,
            Snowflake channelId,
            Snowflake messageId,
            string? message,
            DateTimeOffset createdAt,
            DateTimeOffset dueAt
        )
        {
            Id = id;
            OwnerId = ownerId;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            Message = message;
            CreatedAt = createdAt;
            DueAt = dueAt;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuReminder>
        {
            public void Configure(EntityTypeBuilder<SbuReminder> builder)
            {
                builder.HasKey(t => t.Id);
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
                    .HasForeignKey(r => r.OwnerId)
                    .HasPrincipalKey(m => m.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasOne(r => r.Guild)
                    .WithMany(g => g.Reminders)
                    .HasForeignKey(r => r.GuildId)
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

#endregion
    }
}