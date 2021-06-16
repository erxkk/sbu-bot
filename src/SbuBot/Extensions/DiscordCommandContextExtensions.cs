using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Disqord.Bot;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SbuBot.Commands;
using SbuBot.Models;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DiscordCommandContextExtensions
    {
        public static async ValueTask<SbuMember> GetOrCreateMemberAsync(
            this DiscordGuildCommandContext @this,
            Func<IQueryable<SbuMember>, IQueryable<SbuMember>>? additionalConstraints = null
        )
        {
            IQueryable<SbuMember> query = @this.GetSbuDbContext().Members;

            if (additionalConstraints is { })
            {
                query = additionalConstraints(query);
            }

            var sbuMember = await query.FirstOrDefaultAsync(m => m.DiscordId == @this.Author.Id);

            if (sbuMember is null)
            {
                sbuMember = new(@this.Author, (await @this.GetOrCreateGuildAsync()).Id);
                @this.GetSbuDbContext().Members.Add(sbuMember);
                await @this.GetSbuDbContext().SaveChangesAsync();
            }

            return sbuMember;
        }

        public static async ValueTask<SbuGuild> GetOrCreateGuildAsync(
            this DiscordGuildCommandContext @this,
            Func<IQueryable<SbuGuild>, IQueryable<SbuGuild>>? additionalConstraints = null
        )
        {
            IQueryable<SbuGuild> query = @this.GetSbuDbContext().Guilds;

            if (additionalConstraints is { })
            {
                query = additionalConstraints(query);
            }

            var sbuGuild = await query.FirstOrDefaultAsync(m => m.DiscordId == @this.GuildId);

            if (sbuGuild is null)
            {
                sbuGuild = new(@this.Guild);
                @this.GetSbuDbContext().Guilds.Add(sbuGuild);
                await @this.GetSbuDbContext().SaveChangesAsync();
            }

            return sbuGuild;
        }

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