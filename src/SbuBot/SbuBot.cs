using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
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
        private readonly SbuBotConfiguration _config;
        public bool IsLocked { get; set; }

        public CachedRole ColorRoleSeparator => this.GetRole(
            SbuBotGlobals.Guild.ID,
            SbuBotGlobals.Roles.Categories.COLOR
        );

        public SbuBot(
            SbuBotConfiguration config,
            IOptions<DiscordBotConfiguration> options,
            ILogger<SbuBot> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client)
            => _config = config;

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
            Commands.AddTypeParser(new ReminderDescriptorTypeParser());
            Commands.AddTypeParser(new TagDescriptorTypeParser());

            return base.AddTypeParsersAsync(cancellationToken);
        }

        protected override ValueTask<bool> CheckMessageAsync(IGatewayUserMessage message)
        {
            if (message.Author.Id != SbuBotGlobals.Bot.OWNER_ID && (!_config.IsProduction || IsLocked))
                return ValueTask.FromResult(false);

            return base.CheckMessageAsync(message);
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
                    commandBuilder.Priority -= (commandBuilder.Parameters[0].Type == typeof(string) ? 2 : 1);
            }

            base.MutateModule(moduleBuilder);
        }
    }
}