using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Bot.Hosting;
using Disqord.Gateway;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbuBot.Models;

namespace SbuBot.Services
{
    public sealed class SbuDbConsistencyService : DiscordBotService
    {
        public override int Priority => int.MaxValue;

        public SbuDbConsistencyService(
            ILogger<SbuDbConsistencyService> logger,
            DiscordBotBase bot
        ) : base(logger, bot) { }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            await using (SbuDbContext context = Bot.Services.CreateScope()
                .ServiceProvider.GetRequiredService<SbuDbContext>())
            {
                context.Members.Update(
                    await context.Members.FirstOrDefaultAsync(m => m.DiscordId == e.Member.Id) ?? new(e.Member.Id)
                );

                await context.SaveChangesAsync();
            }

            await base.OnMemberJoined(e);
        }
    }
}