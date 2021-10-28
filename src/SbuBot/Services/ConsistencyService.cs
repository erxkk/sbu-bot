using System.Threading.Tasks;

using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class ConsistencyService : DiscordBotService
    {
        public override int Priority => int.MaxValue;

        private async Task TryAddGuild(IGuild guild)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.GetGuildAsync(guild.Id) is null)
                {
                    context.AddGuild(guild);
                    await context.SaveChangesAsync();

                    Logger.LogDebug("Guild inserted: {@Guild}", new { guild.Id });
                }
            }
        }

        protected override async ValueTask OnJoinedGuild(JoinedGuildEventArgs e)
        {
            await TryAddGuild(e.Guild);
        }

        protected override async ValueTask OnGuildAvailable(GuildAvailableEventArgs e)
        {
            await TryAddGuild(e.Guild);
        }

        protected override async ValueTask OnLeftGuild(LeftGuildEventArgs e)
        {
            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                ReminderService service = scope.ServiceProvider.GetRequiredService<ReminderService>();

                if (await context.GetGuildAsync(e.GuildId) is not { } guild)
                    return;

                context.Guilds.Remove(guild);
                await service.CancelAsync(r => r.GuildId == guild.Id);
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Guild removed: {@Guild}", new { Id = e.GuildId });
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            if (e.Member.IsBot)
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.GetMemberAsync(e.Member.Id, e.GuildId) is { })
                    return;

                context.AddMember(e.Member);
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Member inserted: {@Member}", new { e.Member.Id, Guild = e.GuildId });
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            if (e.User.IsBot)
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();
                ReminderService service = scope.ServiceProvider.GetRequiredService<ReminderService>();

                if (await context.GetMemberAsync(e.User.Id, e.GuildId) is not { } member)
                    return;

                await service.CancelAsync(r => r.OwnerId == member.Id);
                await context.SaveChangesAsync();
                Logger.LogDebug("Member removed: {@Member}", new { e.User.Id, Guild = e.GuildId });
            }
        }

        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            if (e.GuildId is null || e.Member.IsBot)
                return;

            using (IServiceScope scope = Bot.Services.CreateScope())
            {
                SbuDbContext context = scope.ServiceProvider.GetRequiredService<SbuDbContext>();

                if (await context.GetMemberAsync(e.Member.Id, e.GuildId.Value) is { })
                    return;

                context.Members.Add(new(e.Member));
                await context.SaveChangesAsync();
            }

            Logger.LogDebug("Member inserted: {@Member}", new { e.Member.Id, Guild = e.GuildId });
        }
    }
}