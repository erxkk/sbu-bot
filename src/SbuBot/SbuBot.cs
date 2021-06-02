using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Bot.Parsers;
using Disqord.Gateway;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Qmmands;

using SbuBot.Commands;
using SbuBot.Commands.TypeParsers;

namespace SbuBot
{
    public sealed class SbuBot : DiscordBot
    {
        public SbuBot(
            IOptions<DiscordBotConfiguration> options,
            ILogger<SbuBot> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client) { }

        protected override ValueTask AddTypeParsersAsync(CancellationToken cancellationToken = new())
        {
            Commands.AddTypeParser(new ColorRoleTypeParser());
            Commands.AddTypeParser(new DateTimeTypeParser());
            Commands.AddTypeParser(new GuidTypeParser());
            Commands.AddTypeParser(new MemberTypeReader());
            Commands.AddTypeParser(new ReminderTypeParser());
            Commands.AddTypeParser(new TagTypeParser());
            Commands.AddTypeParser(new TimeSpanTypeParser());
            Commands.AddTypeParser(new MessageTypeParser());
            Commands.AddTypeParser(new UserMessageTypeParser());

            return base.AddTypeParsersAsync(cancellationToken);
        }

        public override DiscordCommandContext CreateCommandContext(
            IPrefix prefix,
            string input,
            IGatewayUserMessage message,
            CachedTextChannel channel
        ) => new SbuCommandContext(
            (base.CreateCommandContext(prefix, input, message, channel) as DiscordGuildCommandContext)!
        );

        protected override async ValueTask<bool> BeforeExecutedAsync(DiscordCommandContext context)
        {
            await (context as SbuCommandContext)!.InitializeAsync();
            return await base.BeforeExecutedAsync(context);
        }

        public override ValueTask<bool> IsOwnerAsync(Snowflake userId) => new(userId == SbuBotGlobals.Bot.OWNER_ID);

        protected override void MutateModule(ModuleBuilder moduleBuilder)
        {
            foreach (CommandBuilder commandBuilder in CommandUtilities.EnumerateAllCommands(moduleBuilder))
            {
                // last parameter always remainder
                if (commandBuilder.Parameters.LastOrDefault() is { IsMultiple: false } parameterBuilder)
                    parameterBuilder.IsRemainder = true;

                // avoid command shadowing
                if (commandBuilder.Aliases.Count == 0 && commandBuilder.Parameters.Count == 1)
                    commandBuilder.Priority--;
            }

            base.MutateModule(moduleBuilder);
        }
    }
}