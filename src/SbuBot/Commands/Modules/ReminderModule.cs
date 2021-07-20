using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Kkommon;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Parsing;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Commands.Views;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("reminder", "remind")]
    [Description("A collection of commands for creating modifying and removing reminders.")]
    [Remarks("Reminder timestamps may be given as human readable timespans or strictly colon `:` separated integers.")]
    public sealed class ReminderModule : SbuModuleBase
    {
        [Group("create", "me")]
        [Description("Creates a new reminder with the given timestamp and optional message.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            [Usage(
                "reminder create in 3 days :: do the thing",
                "reminder make tomorrow at 15:00 :: do the thing",
                "remind me in 3 days :: do the thing"
            )]
            public async Task<DiscordCommandResult> CreateAsync(
                [Description("The reminder descriptor.")]
                ReminderDescriptor descriptor
            )
            {
                SbuReminder newReminder = new(
                    Context,
                    Context.Author.Id,
                    Context.Guild.Id,
                    descriptor.Message,
                    descriptor.Timestamp
                );

                await Context.Services.GetRequiredService<ReminderService>().ScheduleAsync(newReminder);

                return Reply(
                    new LocalEmbed()
                        .WithTitle("Reminder Scheduled")
                        .WithDescription(descriptor.Message)
                        .WithFooter("Due")
                        .WithTimestamp(newReminder.DueAt)
                );
            }

            [Command]
            [Usage("reminder create do the thing", "reminder make do the thing", "remind me do the thing")]
            public async Task<DiscordCommandResult> CreateInteractiveAsync(
                [Maximum(SbuReminder.MAX_MESSAGE_LENGTH)][Description("The optional message of the reminder.")]
                string? message = null
            )
            {
                TypeParser<DateTime> parser = Context.Bot.Commands.GetTypeParser<DateTime>();

                string? timestamp;

                switch (await Context.WaitFollowUpForAsync("When do you want to be reminded?"))
                {
                    case Result<string, FollowUpError>.Success followUp:
                        timestamp = followUp.Value;
                        break;

                    case Result<string, FollowUpError>.Error error:
                        return Reply(
                            error.Value == FollowUpError.Aborted
                                ? "Aborted."
                                : "Aborted: You did not provide a timestamp."
                        );

                    // unreachable
                    default:
                        throw new();
                }

                TypeParserResult<DateTime>? parseResult = await parser.ParseAsync(
                    null,
                    timestamp,
                    Context
                );

                if (!parseResult.IsSuccessful)
                    return Reply($"Aborted: {parseResult.FailureReason}.");

                SbuReminder newReminder = new(Context, Context.Author.Id, Context.Guild.Id, message, parseResult.Value);

                await Context.Services.GetRequiredService<ReminderService>().ScheduleAsync(newReminder);

                return Reply(
                    new LocalEmbed()
                        .WithTitle("Reminder Scheduled")
                        .WithDescription(message)
                        .WithFooter("Due")
                        .WithTimestamp(newReminder.DueAt)
                );
            }
        }

        [Command("list")]
        [Description("Lists the given reminder or all if non is given.")]
        [Usage("reminder list", "reminder list last", "reminder list 936DA01F-9ABD-4d 9d-80C7-02AF85C822A8")]
        public DiscordCommandResult List(
            [AuthorMustOwn]
            [Description("The reminder that should be listed.")]
            [Remarks("Lists all reminders if none is specified.")]
            SbuReminder? reminder = null
        )
        {
            if (reminder is { })
            {
                return Reply(
                    new LocalEmbed()
                        .WithTitle("Reminder")
                        .WithDescription(reminder.Message)
                        .WithFooter("Due")
                        .WithTimestamp(reminder.DueAt)
                );
            }

            if (Context.Services.GetRequiredService<ReminderService>().GetReminders()
                is not { Count: > 0 } reminders)
                return Reply("You have no reminders.");

            return DistributedPages(
                reminders.Values
                    .Where(r => r.OwnerId == Context.Author.Id)
                    .Select(
                        r => string.Format(
                            "[`{0}`]({1}) {2}\n{3}\n",
                            r.MessageId,
                            r.JumpUrl,
                            Markdown.Timestamp(r.DueAt),
                            r.Message is { } ? $"`{r.Message}`" : "No Message"
                        )
                    ),
                embedFactory: embed => embed.WithTitle("Your Reminders")
            );
        }

        [Command("edit")]
        [Description("Reschedules the given reminder.")]
        [Usage("reminder edit last in 2 days", "reminder change 936DA01F in 5 seconds")]
        public async Task<DiscordCommandResult> RescheduleAsync(
            [AuthorMustOwn][Description("The reminder to reschedule.")]
            SbuReminder reminder,
            [MustBeFuture][Description("The new timestamp.")]
            DateTime newTimestamp
        )
        {
            await Context.Services.GetRequiredService<ReminderService>()
                .RescheduleAsync(reminder.MessageId, newTimestamp);

            return Reply(
                new LocalEmbed()
                    .WithTitle("Reminder Rescheduled")
                    .WithDescription(reminder.Message)
                    .WithFooter("Due")
                    .WithTimestamp(newTimestamp)
            );
        }

        [Command("delete")]
        [Description("Cancels the given reminder.")]
        [Usage("reminder remove last", "reminder delete 936DA01F", "reminder cancel all")]
        public async Task<DiscordCommandResult> CancelAsync(
            [AuthorMustOwn][Description("The reminder that should be canceled.")]
            OneOrAll<SbuReminder> reminder
        )
        {
            switch (reminder)
            {
                case OneOrAll<SbuReminder>.All:
                {
                    ConfirmationState confirmationResult = await ConfirmationAsync(
                        "Reminder Removal",
                        "Are you sure you want to cancel all your reminders?"
                    );

                    switch (confirmationResult)
                    {
                        case ConfirmationState.None:
                        case ConfirmationState.Aborted:
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            break;

                        // unreachable
                        default:
                            throw new();
                    }

                    await Context.Services.GetRequiredService<ReminderService>()
                        .CancelAsync(r => r.OwnerId == Context.Author.Id);

                    return Reply("Cancelled all reminders.");
                }

                case OneOrAll<SbuReminder>.Specific specific:
                {
                    ConfirmationState confirmationResult = await ConfirmationAsync(
                        "Reminder Removal",
                        $"Are you sure you want to cancel `{specific.Value.MessageId}`?"
                    );

                    switch (confirmationResult)
                    {
                        case ConfirmationState.None:
                        case ConfirmationState.Aborted:
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            break;

                        // unreachable
                        default:
                            throw new();
                    }

                    await Context.Services.GetRequiredService<ReminderService>().CancelAsync(specific.Value.MessageId);

                    return Reply(
                        new LocalEmbed()
                            .WithTitle("Reminder Cancelled")
                            .WithDescription(specific.Value.Message)
                            .WithFooter("Cancelled")
                            .WithCurrentTimestamp()
                    );
                }

                // unreachable
                default:
                    throw new();
            }
        }
    }
}