using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Gateway;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SbuBot.Models;

namespace SbuBot.Commands
{
    public sealed class SbuCommandContext : DiscordGuildCommandContext
    {
        private SbuDbContext? _dbContext;

        public override SbuBot Bot => (base.Bot as SbuBot)!;

        public SbuDbContext Db => _dbContext ??= Services.GetRequiredService<SbuDbContext>();

        public SbuMember Invoker { get; private set; }

        public SbuCommandContext(
            DiscordBotBase bot,
            IPrefix prefix,
            string input,
            IGatewayUserMessage message,
            CachedTextChannel channel,
            IServiceProvider services,
            SbuMember invoker = null!
        ) : base(bot, prefix, input, message, channel, services)
            => Invoker = invoker;

        public SbuCommandContext(
            DiscordBotBase bot,
            IPrefix prefix,
            string input,
            IGatewayUserMessage message,
            CachedTextChannel channel,
            IServiceScope serviceScope,
            SbuMember invoker = null!
        ) : base(bot, prefix, input, message, channel, serviceScope)
            => Invoker = invoker;

        public SbuCommandContext(DiscordGuildCommandContext context, SbuMember invoker = null!) : base(
            context.Bot,
            context.Prefix,
            context.Input,
            context.Message,
            context.Channel,
            context.Services
        ) => Invoker = invoker;

        public async Task InitializeAsync()
        {
            Invoker ??= await Db.Members.Include(m => m.ColorRole).FirstOrDefaultAsync(m => m.DiscordId == Author.Id);

            if (Invoker is null)
            {
                Invoker = new(CurrentMember);
                Db.Members.Add(Invoker);
                await Db.SaveChangesAsync();
            }
        }
    }
}