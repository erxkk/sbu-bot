using System;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed record SbuReminder : SbuEntity
    {
        public Snowflake OwnerId { get; }
        public Snowflake ChannelId { get; }
        public Snowflake MessageId { get; }
        public string? Message { get; set; }

        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset DueAt { get; set; }
        public DateTimeOffset IsDispatched { get; set; }

        // nav properties
        public SbuMember Owner { get; }

        // new
        public SbuReminder(
            Snowflake ownerId,
            Snowflake channelId,
            Snowflake messageId,
            string? message,
            DateTimeOffset createdAt,
            DateTimeOffset dueAt
        )
        {
            OwnerId = ownerId;
            ChannelId = channelId;
            MessageId = messageId;
            Message = message;
            CreatedAt = createdAt;
            DueAt = dueAt;
        }

        // ef core
        internal SbuReminder(
            Guid id,
            Snowflake ownerId,
            Snowflake channelId,
            Snowflake messageId,
            string? message,
            DateTimeOffset createdAt,
            DateTimeOffset dueAt
        ) : base(id)
        {
            OwnerId = ownerId;
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

                builder.Property(t => t.ChannelId);
                builder.Property(t => t.MessageId);

                builder.Property(t => t.Message)
                    .HasMaxLength(1024);

                builder.Property(t => t.CreatedAt);
                builder.Property(t => t.DueAt);
                builder.Property(t => t.IsDispatched);

                builder.HasOne(r => r.Owner)
                    .WithMany(m => m.Reminders)
                    .HasForeignKey(r => r.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId);
            }
        }
    }
}