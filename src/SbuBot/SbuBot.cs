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
        public bool IsLocked { get; set; }

        public SbuBot(
            SbuBotConfiguration config,
            IOptions<DiscordBotConfiguration> options,
            ILogger<SbuBot> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client)
            => IsLocked = !config.IsProduction;

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
            if (message.Author.Id == SbuGlobals.Bot.OWNER)
                return ValueTask.FromResult(true);

            if (IsLocked)
                return ValueTask.FromResult(false);

            return base.CheckMessageAsync(message);
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
                // [Remarks("literal" + constantNonString)] will not work unless the value is a const string
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