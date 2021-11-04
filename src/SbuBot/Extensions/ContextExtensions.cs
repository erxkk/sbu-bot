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
        public static SbuDbContext GetSbuDbContext(this DiscordCommandContext @this)
            => @this.Services.GetRequiredService<SbuDbContext>();

        public static Task<SbuMember?> GetDbMemberAsync(this DiscordGuildCommandContext @this, IMember member)
            => @this.GetSbuDbContext().GetMemberFullAsync(member);

        // will not return null, consistency service runs before command
        public static Task<SbuMember> GetDbAuthorAsync(this DiscordGuildCommandContext @this)
            => @this.GetDbMemberAsync(@this.Author)!;

        public static Task<SbuColorRole?> GetDbColorRoleAsync(this DiscordGuildCommandContext @this, IRole role)
            => @this.GetSbuDbContext().GetColorRoleFullAsync(role);

        // will not return null, consistency service runs before command
        public static Task<SbuGuild> GetDbGuildAsync(this DiscordGuildCommandContext @this)
            => @this.GetSbuDbContext().GetGuildAsync(@this.Guild)!;

        public static Task<SbuTag?> GetTagAsync(this DiscordGuildCommandContext @this, string name)
            => @this.GetSbuDbContext().GetTagFullAsync(name, @this.GuildId);

        public static Task<List<SbuTag>> GetTagsAsync(this DiscordGuildCommandContext @this)
            => @this.GetSbuDbContext()
                .Tags
                .Where(t => t.GuildId == @this.GuildId)
                .ToListAsync(@this.Bot.StoppingToken);

        public static Task<List<SbuTag>> GetTagsFullAsync(this DiscordGuildCommandContext @this)
            => @this.GetSbuDbContext()
                .Tags
                .Include(t => t.Guild)
                .Include(t => t.Owner)
                .Where(t => t.GuildId == @this.GuildId)
                .ToListAsync(@this.Bot.StoppingToken);

        public static Task<SbuAutoResponse?> GetAutoResponseAsync(
            this DiscordGuildCommandContext @this,
            string trigger
        ) => @this.GetSbuDbContext().GetAutoResponseAsync(trigger, @this.GuildId);

        public static Task<SbuAutoResponse?> GetAutoResponseFullAsync(
            this DiscordGuildCommandContext @this,
            string trigger
        ) => @this.GetSbuDbContext().GetAutoResponseFullAsync(trigger, @this.GuildId);

        public static Task<List<SbuAutoResponse>> GetAutoResponsesAsync(this DiscordGuildCommandContext @this)
            => @this.GetSbuDbContext()
                .AutoResponses
                .Include(t => t.Guild)
                .Where(t => t.GuildId == @this.GuildId)
                .ToListAsync(@this.Bot.StoppingToken);

        public static Task<List<SbuAutoResponse>> GetAutoResponsesFullAsync(this DiscordGuildCommandContext @this)
            => @this.GetSbuDbContext()
                .AutoResponses
                .Include(t => t.Guild)
                .Where(t => t.GuildId == @this.GuildId)
                .ToListAsync(@this.Bot.StoppingToken);

        public static async Task<Result<string, FollowUpError>> WaitFollowUpForAsync(
            this DiscordGuildCommandContext @this,
            string prompt,
            bool yield = false
        )
        {
            await @this.Channel.SendMessageAsync(
                new LocalMessage()
                    .WithContent(prompt)
                    .WithReference(new LocalMessageReference().WithMessageId(@this.Message.Id))
            );

            MessageReceivedEventArgs? args;

            if (yield)
            {
                await using (@this.BeginYield())
                {
                    args = await @this.WaitForMessageAsync();
                }
            }
            else
            {
                args = await @this.WaitForMessageAsync();
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
}