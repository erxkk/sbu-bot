using System;
using System.Diagnostics.CodeAnalysis;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SbuBot.Commands;

namespace SbuBot.Models
{
    public sealed class SbuReminder : SbuEntityBase, ISbuOwnedEntity
    {
        public const int MAX_MESSAGE_LENGTH = 1024;

        [NotNull]
        public Snowflake? OwnerId { get; set; }

        public Snowflake ChannelId { get; }
        public Snowflake MessageId { get; }
        public string? Message { get; }

        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset DueAt { get; set; }

        [HideOnSerialize, NotLogged]
        public bool IsDispatched { get; set; }

        [HideOnSerialize, NotLogged]
        public string JumpUrl => string.Format(
            "https://discord.com/channels/{0}/{1}/{2}",
            SbuGlobals.Guild.SELF,
            ChannelId,
            MessageId
        );

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuMember? Owner { get; }

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

        public SbuReminder(
            SbuCommandContext context,
            string? message,
            DateTimeOffset dueAt
        )
        {
            OwnerId = context.Author.Id;
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
            Snowflake channelId,
            Snowflake messageId,
            string? message,
            DateTimeOffset createdAt,
            DateTimeOffset dueAt,
            bool isDispatched
        ) : base(id)
        {
            OwnerId = ownerId;
            ChannelId = channelId;
            MessageId = messageId;
            Message = message;
            CreatedAt = createdAt;
            DueAt = dueAt;
            IsDispatched = isDispatched;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuReminder>
        {
            public void Configure(EntityTypeBuilder<SbuReminder> builder)
            {
                builder.HasKey(t => t.Id);
                builder.HasIndex(t => t.OwnerId);

                builder.Property(t => t.ChannelId);
                builder.Property(t => t.MessageId);
                builder.Property(t => t.Message).HasMaxLength(SbuReminder.MAX_MESSAGE_LENGTH);
                builder.Property(t => t.CreatedAt);
                builder.Property(t => t.DueAt);
                builder.Property(t => t.IsDispatched);

                builder.HasOne(r => r.Owner)
                    .WithMany(m => m.Reminders)
                    .HasForeignKey(r => r.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

#endregion
    }
}