using System;
using System.ComponentModel;

using Disqord;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DiscordExtensions
    {
        public static LocalEmbed WithCurrentTimestamp(this LocalEmbed @this)
            => @this.WithTimestamp(DateTimeOffset.Now);

        public static LocalEmbed AddInlineField(this LocalEmbed @this, string name, string content)
            => @this.AddField(name, content, true);

        public static LocalEmbed AddInlineField(this LocalEmbed @this, string name, object content)
            => @this.AddField(name, content, true);

        public static LocalEmbed AddBlankInlineField(this LocalEmbed @this)
            => @this.AddBlankField(true);
    }
}