using System;
using System.ComponentModel;
using System.Linq;

using Disqord;
using Disqord.Gateway;

using Kkommon;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DiscordExtensions
    {
        public static LocalEmbed WithCurrentTimestamp(this LocalEmbed @this)
        {
            Preconditions.NotNull(@this, nameof(@this));

            return @this.WithTimestamp(DateTimeOffset.Now);
        }

        public static LocalEmbed AddInlineField(this LocalEmbed @this, string name, string content)
        {
            Preconditions.NotNull(@this, nameof(@this));

            return @this.AddField(name, content, true);
        }

        public static LocalEmbed AddInlineField(this LocalEmbed @this, string name, object content)
        {
            Preconditions.NotNull(@this, nameof(@this));

            return @this.AddField(name, content, true);
        }

        public static LocalEmbed AddBlankInlineField(this LocalEmbed @this)
        {
            Preconditions.NotNull(@this, nameof(@this));

            return @this.AddBlankField(true);
        }

        public static int CustomEmojiSlots(this IGuild @this)
        {
            Preconditions.NotNull(@this, nameof(@this));

            return @this.BoostTier switch
            {
                GuildBoostTier.None => 50,
                GuildBoostTier.First => 100,
                GuildBoostTier.Second => 150,
                GuildBoostTier.Third => 250,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public static IRole? GetColorRole(this IMember @this)
        {
            Preconditions.NotNull(@this, nameof(@this));

            return @this.GetRoles()
                .Values
                .OrderByDescending(role => role.Position)
                .FirstOrDefault(role => role.Color is { });
        }
    }
}