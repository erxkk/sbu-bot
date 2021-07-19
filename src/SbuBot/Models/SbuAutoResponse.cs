using System;
using System.Linq;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuAutoResponse : ISbuGuildEntity
    {
        public const int MAX_LENGTH = 1024;

        public Snowflake GuildId { get; }
        public string Trigger { get; }
        public string Response { get; }

        [NotLogged]
        public SbuGuild? Guild { get; }

        public SbuAutoResponse(IGuild guild, string trigger, string response)
        {
            GuildId = guild.Id;
            Trigger = trigger;
            Response = response;
        }

#region EFCore

        public SbuAutoResponse(Snowflake guildId, string trigger, string response)
        {
            GuildId = guildId;
            Trigger = trigger;
            Response = response;
        }

#nullable disable
        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuAutoResponse>
        {
            public void Configure(EntityTypeBuilder<SbuAutoResponse> builder)
            {
                builder.HasKey(ar => new { ar.Trigger, ar.GuildId });
                builder.HasIndex(ar => ar.Trigger);
                builder.HasIndex(ar => ar.GuildId);

                builder.Property(ar => ar.GuildId);
                builder.Property(ar => ar.Trigger).HasMaxLength(MAX_LENGTH).IsRequired();
                builder.Property(ar => ar.Response).HasMaxLength(MAX_LENGTH).IsRequired();

                builder.HasOne(ar => ar.Guild)
                    .WithMany(g => g.AutoResponses)
                    .HasForeignKey(ar => ar.GuildId)
                    .HasPrincipalKey(g => g.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

#endregion

#region Validation

        public enum ValidTriggerType
        {
            Valid,
            TooLong,
            Reserved,
        }

        public static ValidTriggerType IsValidTrigger(string trigger)
        {
            return trigger.Length switch
            {
                > MAX_LENGTH => ValidTriggerType.TooLong,
                _ => SbuGlobals.Keywords.ALL_RESERVED.Any(rn => rn.Equals(trigger, StringComparison.OrdinalIgnoreCase))
                    ? ValidTriggerType.Reserved
                    : ValidTriggerType.Valid,
            };
        }

        public enum ValidResponseType
        {
            Valid,
            TooLong,
        }

        public static ValidResponseType IsValidResponse(string response)
        {
            return response.Length switch
            {
                > MAX_LENGTH => ValidResponseType.TooLong,
                _ => ValidResponseType.Valid,
            };
        }

#endregion
    }
}