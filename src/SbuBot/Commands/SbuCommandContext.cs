using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord.Bot;
using Disqord.Gateway;

using Kkommon;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SbuBot.Models;

namespace SbuBot.Commands
{
    public sealed class SbuCommandContext : DiscordGuildCommandContext
    {
        private SbuDbContext? _dbContext;
        private SbuGuild? _sbuGuild;
        private SbuMember? _sbuMember;
        private readonly List<AsyncAction<SbuCommandContext>> _actions = new(1);

        public override SbuBot Bot => (base.Bot as SbuBot)!;
        public SbuDbContext Db => _dbContext ??= Services.GetRequiredService<SbuDbContext>();

        public SbuCommandContext(
            DiscordBotBase bot,
            IPrefix prefix,
            string input,
            IGatewayUserMessage message,
            CachedTextChannel channel,
            IServiceProvider services,
            SbuMember? sbuMember = null
        ) : base(bot, prefix, input, message, channel, services)
            => _sbuMember = sbuMember;

        public SbuCommandContext(
            DiscordBotBase bot,
            IPrefix prefix,
            string input,
            IGatewayUserMessage message,
            CachedTextChannel channel,
            IServiceScope serviceScope,
            SbuMember? sbuMember = null
        ) : base(bot, prefix, input, message, channel, serviceScope)
            => _sbuMember = sbuMember;

        public SbuCommandContext(DiscordGuildCommandContext context, SbuMember? sbuMember = null) : base(
            context.Bot,
            context.Prefix,
            context.Input,
            context.Message,
            context.Channel,
            context.Services
        ) => _sbuMember = sbuMember;

        public async Task<SbuMember> GetOrCreateMemberAsync(
            Func<IQueryable<SbuMember>, IQueryable<SbuMember>>? additionalConstraints = null
        )
        {
            IQueryable<SbuMember> query = Db.Members;

            if (additionalConstraints is { })
            {
                query = additionalConstraints(query);
            }

            _sbuMember ??= await query.FirstOrDefaultAsync(m => m.DiscordId == Author.Id);

            if (_sbuMember is null)
            {
                _sbuMember = new(Author, (await GetOrCreateGuildAsync()).Id);
                Db.Members.Add(_sbuMember);
                await Db.SaveChangesAsync();
            }

            return _sbuMember;
        }

        public async Task<SbuGuild> GetOrCreateGuildAsync(
            Func<IQueryable<SbuGuild>, IQueryable<SbuGuild>>? additionalConstraints = null
        )
        {
            IQueryable<SbuGuild> query = Db.Guilds;

            if (additionalConstraints is { })
            {
                query = additionalConstraints(query);
            }

            _sbuGuild ??= await query.FirstOrDefaultAsync(m => m.DiscordId == Author.Id);

            if (_sbuMember is null)
            {
                _sbuGuild = new(Guild);
                Db.Guilds.Add(_sbuGuild);
                await Db.SaveChangesAsync();
            }

            return _sbuGuild;
        }

        public void RepostAsAlias(string alias) => Bot.Queue.Post(
            new SbuCommandContext(
                Bot,
                Prefix,
                alias,
                new ProxyMessage(Message, alias, Author, Channel.Id),
                Channel,
                Services
            ),
            context => context.Bot.ExecuteAsync(context)
        );

        public void AttachCleanUp(AsyncAction<SbuCommandContext> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            _actions.Add(action);
        }

        public override async ValueTask DisposeAsync()
        {
            foreach (AsyncAction<SbuCommandContext> func in _actions)
                await func(this);

            await base.DisposeAsync();
        }
    }
}