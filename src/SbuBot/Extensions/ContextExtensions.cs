using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SbuBot.Commands;
using SbuBot.Models;

namespace SbuBot.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContextExtensions
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

        public static Task<SbuMember> GetMemberAsync(this DiscordGuildCommandContext @this, IMember member)
            => @this.GetSbuDbContext()
                .GetMemberAsync(member, m => m.Include(m => m.Guild).Include(m => m.ColorRole));

        public static Task<SbuMember> GetAuthorAsync(this DiscordGuildCommandContext @this)
            => @this.GetMemberAsync(@this.Author);

        public static Task<SbuColorRole> GetColorRoleAsync(this DiscordGuildCommandContext @this, IRole role)
            => @this.GetSbuDbContext().GetColorRoleAsync(role, r => r.Include(r => r.Guild).Include(r => r.Owner));

        public static Task<SbuGuild> GetGuildAsync(this DiscordGuildCommandContext @this)
            => @this.GetSbuDbContext().GetGuildAsync(@this.Guild);

        public static Task<SbuTag?> GetTagAsync(this DiscordGuildCommandContext @this, string name)
            => @this.GetSbuDbContext()
                .GetTagAsync(name, @this.GuildId, t => t.Include(t => t.Guild).Include(t => t.Owner));

        public static Task<List<SbuTag>> GetTagsAsync(this DiscordGuildCommandContext @this)
            => @this.GetSbuDbContext()
                .Tags
                .Include(t => t.Guild)
                .Include(t => t.Owner)
                .Where(t => t.GuildId == @this.GuildId)
                .ToListAsync(@this.Bot.StoppingToken);

        public static Task<int> SaveChangesAsync(this DiscordGuildCommandContext @this)
            => @this.GetSbuDbContext().SaveChangesAsync();

        public static async Task<ConfirmationResult> WaitForConfirmationAsync(
            this DiscordGuildCommandContext @this,
            string prompt
        )
        {
            await @this.Channel.SendMessageAsync(new LocalMessage().WithContent(prompt).WithReply(@this.Message));
            MessageReceivedEventArgs? args;

            await using (_ = @this.BeginYield())
            {
                args = await @this.WaitForMessageAsync(a => a.Member.Id == @this.Author.Id);
            }

            return args switch
            {
                { } received when received.Message.Content.Equals("no", StringComparison.OrdinalIgnoreCase)
                    || received.Message.Content.Equals("abort", StringComparison.OrdinalIgnoreCase) =>
                    ConfirmationResult.Aborted,
                { } received when received.Message.Content.Equals("yes", StringComparison.OrdinalIgnoreCase)
                    || received.Message.Content.Equals("confirm", StringComparison.OrdinalIgnoreCase) =>
                    ConfirmationResult.Confirmed,
                _ => ConfirmationResult.Timeout,
            };
        }

        public static async Task<Result<string, FollowUpError>> WaitFollowUpForAsync(
            this DiscordGuildCommandContext @this,
            string prompt
        )
        {
            await @this.Channel.SendMessageAsync(new LocalMessage().WithContent(prompt).WithReply(@this.Message));
            MessageReceivedEventArgs? args;

            await using (_ = @this.BeginYield())
            {
                args = await @this.WaitForMessageAsync(a => a.Member.Id == @this.Author.Id);
            }

            return args switch
            {
                { } received => !received.Message.Content.Equals("abort", StringComparison.OrdinalIgnoreCase)
                    ? new Result<string, FollowUpError>.Success(received.Message.Content)
                    : new Result<string, FollowUpError>.Error(FollowUpError.Aborted),
                _ => new Result<string, FollowUpError>.Error(FollowUpError.Timeout),
            };
        }
    }

    public enum ConfirmationResult
    {
        Timeout,
        Aborted,
        Confirmed,
    }

    public enum FollowUpError
    {
        Timeout,
        Aborted,
    }
}