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
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Commands.Parsing.TypeParsers;
using SbuBot.Models;

namespace SbuBot
{
    public sealed class SbuBot : DiscordBot
    {
        public bool IsLocked { get; set; }

        public SbuBot(
            SbuConfiguration config,
            IOptions<DiscordBotConfiguration> options,
            ILogger<SbuBot> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client)
            => IsLocked = !config.IsProduction;

        protected override ValueTask AddTypeParsersAsync(CancellationToken cancellationToken = default)
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

            Commands.AddTypeParser(new OneOrAllTypeParser<SbuReminder>());
            Commands.AddTypeParser(new OneOrAllTypeParser<SbuTag>());

            return base.AddTypeParsersAsync(cancellationToken);
        }

        protected override ValueTask<bool> CheckMessageAsync(IGatewayUserMessage message)
        {
            if (message.Author.Id == SbuGlobals.Bot.OWNER)
                return new(true);

            if (IsLocked)
                return new(false);

            return base.CheckMessageAsync(message);
        }

        protected override string? FormatFailureReason(DiscordCommandContext context, FailedResult result)
        {
            return result switch
            {
                CommandNotFoundResult => null,
                TypeParseFailedResult parseFailedResult => string.Format(
                    "Type parse failed for parameter `{0}`:\n• {1}",
                    parseFailedResult.Parameter.Format(false),
                    parseFailedResult.FailureReason
                ),
                ChecksFailedResult checksFailed => string.Format(
                    "Checks failed:\n{0}",
                    string.Join('\n', checksFailed.FailedChecks.Select((c => $"• {c.Result.FailureReason}")))
                ),
                ParameterChecksFailedResult parameterChecksFailed => string.Format(
                    "Checks failed for parameter `{0}`:\n{1}",
                    parameterChecksFailed.Parameter.Format(false),
                    string.Join('\n', parameterChecksFailed.FailedChecks.Select((c => $"• {c.Result.FailureReason}")))
                ),
                _ => result.FailureReason,
            };
        }

        protected override LocalMessage? FormatFailureMessage(DiscordCommandContext context, FailedResult result)
        {
            string? description = FormatFailureReason(context, result);

            if (description is null)
                return null;

            LocalEmbed embed = new LocalEmbed().WithDescription(description).WithColor(3092790);

            if (result is OverloadsFailedResult overloadsFailed)
            {
                foreach ((Command overload, FailedResult overloadResult) in overloadsFailed.FailedOverloads)
                {
                    string? reason = FormatFailureReason(context, overloadResult);

                    if (reason is { })
                        embed.AddField(string.Format("Overload: {0}", overload.FullAliases[0]), reason);
                }
            }
            else if (context.Command is { })
            {
                embed.WithTitle(string.Format("Command: {0}", context.Command.FullAliases[0]));
            }

            return new LocalMessage().WithEmbeds(embed);
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

        public override ValueTask SetupAsync(CancellationToken cancellationToken = default)
        {
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.SetObserved();
                Logger.LogError(e.Exception, "Unobserved Exception: {@Sender}", sender);
            };

            return base.SetupAsync(cancellationToken);
        }
    }
}