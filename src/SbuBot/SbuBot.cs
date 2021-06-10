using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Qmmands;

using SbuBot.Commands;
using SbuBot.Commands.Descriptors;
using SbuBot.Commands.TypeParsers;

namespace SbuBot
{
    public sealed class SbuBot : DiscordBot
    {
        private readonly SbuBotConfiguration _config;
        public bool IsLocked { get; set; }

        public CachedRole ColorRoleSeparator => this.GetRole(
            SbuGlobals.Guild.SELF,
            SbuGlobals.Role.Color.SELF
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

        public override ValueTask SetupAsync(CancellationToken cancellationToken = default)
        {
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.SetObserved();
                Logger.LogError(e.Exception, "Unobserved Exception: {@Sender}", sender);
            };

            return base.SetupAsync(cancellationToken);
        }

        protected override ValueTask<bool> CheckMessageAsync(IGatewayUserMessage message)
        {
            if (message.Author.Id != SbuGlobals.Bot.OWNER && (!_config.IsProduction || IsLocked))
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

        public override ValueTask<bool> IsOwnerAsync(Snowflake userId) => new(userId == SbuGlobals.Bot.OWNER);

        protected override void MutateModule(ModuleBuilder moduleBuilder)
        {
            foreach (CommandBuilder commandBuilder in CommandUtilities.EnumerateAllCommands(moduleBuilder))
            {
                // last parameter always remainder
                if (commandBuilder.Parameters.LastOrDefault() is { IsMultiple: false } remainderParameterBuilder)
                    remainderParameterBuilder.IsRemainder = true;

                // avoid command shadowing
                if (commandBuilder.Aliases.Count == 0 && commandBuilder.Parameters.Count == 1)
                    commandBuilder.Priority -= (commandBuilder.Parameters[0].Type == typeof(string) ? 2 : 1);

                // assign remarks dynamically on descriptors to allow for constant integer stringification lmao
                // [Remarks("literal" + constantNonString)] will not work unless the value is specified as string as
                // well, although the string should be pasted upon compilation
                foreach (ParameterBuilder parameterBuilder in commandBuilder.Parameters
                    .Where(p => p.Type.IsAssignableTo(typeof(IDescriptor)))
                )
                {
                    if (parameterBuilder.Type
                            .GetField("REMARKS", BindingFlags.Static | BindingFlags.Public)
                            ?.GetValue(null)
                        is string remarks)
                        parameterBuilder.Remarks = remarks;
                }
            }

            base.MutateModule(moduleBuilder);
        }
    }
}