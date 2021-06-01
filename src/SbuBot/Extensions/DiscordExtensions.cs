using System;
using System.ComponentModel;

using Disqord;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DiscordExtensions
    {
        public static LocalEmbedBuilder WithCurrentTimestamp(this LocalEmbedBuilder @this)
            => @this.WithTimestamp(DateTimeOffset.Now);
    }
}