using System;
using System.Collections.Generic;

using Destructurama.Attributed;

using Disqord;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SbuBot.Models
{
    public sealed class SbuMember : SbuEntityBase, ISbuDiscordEntity
    {
        public Snowflake DiscordId { get; set; }

        [HideOnSerialize]
        public string InheritanceCode { get; } = Utility.GeneratePseudoRandomString();

        // nav properties
        [HideOnSerialize, NotLogged]
        public SbuColorRole? ColorRole { get; }

        [HideOnSerialize, NotLogged]
        public List<SbuTag> Tags { get; } = new();

        [HideOnSerialize, NotLogged]
        public List<SbuReminder> Reminders { get; } = new();

        public SbuMember(Snowflake discordId) => DiscordId = discordId;

        public SbuMember(IMember member) => DiscordId = member.Id;

#region EFCore

        internal SbuMember(Guid id, Snowflake discordId, string inheritanceCode) : base(id)
        {
            DiscordId = discordId;
            InheritanceCode = inheritanceCode;
        }

        internal sealed class EntityTypeConfiguration : IEntityTypeConfiguration<SbuMember>
        {
            public void Configure(EntityTypeBuilder<SbuMember> builder)
            {
                builder.HasKey(m => m.Id);
                builder.HasIndex(m => m.DiscordId).IsUnique();

                builder.Property(m => m.DiscordId);
                builder.Property(m => m.InheritanceCode);

                builder.HasOne(m => m.ColorRole)
                    .WithOne(cr => cr.Owner)
                    .HasForeignKey<SbuColorRole>(cr => cr.OwnerId)
                    .HasPrincipalKey<SbuMember>(m => m.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(m => m.Tags)
                    .WithOne(t => t.Owner)
                    .HasForeignKey(t => t.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(m => m.Reminders)
                    .WithOne(r => r.Owner)
                    .HasForeignKey(r => r.OwnerId)
                    .HasPrincipalKey(m => m.DiscordId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

#endregion
    }
}