using System;
using System.ComponentModel;
using System.Linq;

using Disqord;
using Disqord.Gateway;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DiscordExtensions
    {
        public static int GetHierarchy(this IMember @this)
        {
            if (@this is null)
                throw new ArgumentNullException(nameof(@this));

            return @this.GetRoles().Values.Max(r => r.Position);
        }

        public static LocalEmbed WithCurrentTimestamp(this LocalEmbed @this)
        {
            if (@this is null)
                throw new ArgumentNullException(nameof(@this));

            return @this.WithTimestamp(DateTimeOffset.Now);
        }

        public static LocalEmbed AddInlineField(this LocalEmbed @this, string name, string content)
        {
            if (@this is null)
                throw new ArgumentNullException(nameof(@this));

            return @this.AddField(name, content, true);
        }

        public static LocalEmbed AddInlineField(this LocalEmbed @this, string name, object content)
        {
            if (@this is null)
                throw new ArgumentNullException(nameof(@this));

            return @this.AddField(name, content, true);
        }

        public static LocalEmbed AddBlankInlineField(this LocalEmbed @this)
        {
            if (@this is null)
                throw new ArgumentNullException(nameof(@this));

            return @this.AddBlankField(true);
        }

        public static int CustomEmojiSlots(this IGuild @this)
        {
            if (@this is null)
                throw new ArgumentNullException(nameof(@this));

            return @this.BoostTier switch
            {
                BoostTier.None => 50,
                BoostTier.First => 100,
                BoostTier.Second => 150,
                BoostTier.Third => 250,
                _ => throw new ArgumentOutOfRangeException(nameof(@this), @this.BoostTier, null),
            };
        }

        public static IRole? GetColorRole(this IMember @this)
        {
            if (@this is null)
                throw new ArgumentNullException(nameof(@this));

            return @this.GetRoles()
                .Values.OrderByDescending(role => role.Position)
                .FirstOrDefault(role => role.Color is { });
        }
    }
}