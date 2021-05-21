using System;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed record SbuNicknameLog : SbuEntity
    {
        public DateTimeOffset Timestamp { get; }
        public Snowflake OwnerId { get; }
        public string Nickname { get; }

        // nav properties
        public SbuMember Owner { get; }

        // new
        public SbuNicknameLog(DateTimeOffset timestamp, Snowflake ownerId, string nickname)
        {
            Timestamp = timestamp;
            OwnerId = ownerId;
            Nickname = nickname;
        }

        // ef core
        internal SbuNicknameLog(Guid id, DateTimeOffset timestamp, Snowflake ownerId, string nickname) : base(id)
        {
            Timestamp = timestamp;
            OwnerId = ownerId;
            Nickname = nickname;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuNicknameLog>
        {
            public void Configure(EntityTypeBuilder<SbuNicknameLog> builder)
            {
                builder.HasKey(nl => nl.Id);
                builder.HasIndex(nl => nl.OwnerId);

                builder.Property(nl => nl.Timestamp)
                    .ValueGeneratedOnAdd();

                builder.Property(nl => nl.OwnerId);

                builder.Property(nl => nl.Nickname)
                    .HasMaxLength(32);

                builder.HasOne(nl => nl.Owner)
                    .WithMany(m => m.Nicknames)
                    .HasForeignKey(nl => nl.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId);
            }
        }
    }
}