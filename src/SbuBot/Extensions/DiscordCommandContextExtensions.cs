using System.ComponentModel;

using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using SbuBot.Commands;
using SbuBot.Models;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DiscordCommandContextExtensions
    {
        public static void RepostAsAlias(this DiscordGuildCommandContext @this, string alias) => @this.Bot.Queue.Post(
            new DiscordGuildCommandContext(
                @this.Bot,
                @this.Prefix,
                alias,
                new ProxyMessage(@this.Message, $"{@this.Prefix} {alias}", @this.Author, @this.ChannelId),
                @this.Channel,
                @this.Services
            ),
            context => context.Bot.ExecuteAsync(context)
        );

        public static SbuDbContext GetSbuDbContext(this DiscordCommandContext @this)
            => @this.Services.GetRequiredService<SbuDbContext>();
    }
}