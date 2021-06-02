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
    }
}