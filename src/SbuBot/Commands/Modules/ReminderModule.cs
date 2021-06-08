using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Checks.Parameters;
using SbuBot.Commands.Descriptors;
using SbuBot.Commands.Information;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("reminder", "remind", "remindme", "r")]
    [Description("A collection of commands for creating modifying and removing reminders.")]
    [Remarks("Reminder timestamps may be given as human readable timespans or strictly colon `:` separated integers.")]
    public sealed class ReminderModule : SbuModuleBase
    {
        [Group("create", "make", "new"), PureGroup]
        [Description("Creates a new reminder with the given timestamp and optional message.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> CreateAsync(ReminderDescriptor reminderDescriptor)
            {
                var newReminder = new SbuReminder(Context, reminderDescriptor.Message, reminderDescriptor.Timestamp);

                await Context.Services.GetRequiredService<ReminderService>().ScheduleReminderAsync(newReminder);

                return Reply(
                    new LocalEmbed()
                        .WithTitle("Reminder Scheduled")
                        .WithDescription(reminderDescriptor.Message)
                        .WithFooter("Due")
                        .WithTimestamp(newReminder.DueAt)
                );
            }

            [Command]
            public async Task<DiscordCommandResult> CreateInteractiveAsync(string message)
            {
                TypeParser<DateTime> parser = Context.Bot.Commands.GetTypeParser<DateTime>();
                MessageReceivedEventArgs? waitResult;

                await Reply("When do you want to be reminded?");

                await using (Context.BeginYield())
                {
                    waitResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                }

                if (waitResult is null)
                    return Reply("Aborted: You did not provide a timestamp.");

                TypeParserResult<DateTime>? parseResult = await parser.ParseAsync(
                    null,
                    waitResult.Message.Content,
                    Context
                );

                if (!parseResult.IsSuccessful)
                    return Reply($"Aborted: {parseResult.FailureReason}.");

                var newReminder = new SbuReminder(Context, message, parseResult.Value);

                await using (Context.BeginYield())
                {
                    await Context.Services.GetRequiredService<ReminderService>().ScheduleReminderAsync(newReminder);
                }

                return Reply(
                    new LocalEmbed()
                        .WithTitle("Reminder Scheduled")
                        .WithDescription(message)
                        .WithFooter("Due")
                        .WithTimestamp(newReminder.DueAt)
                );
            }
        }

        [Command("edit", "change")]
        [Description("Reschedules the given reminder.")]
        public async Task<DiscordCommandResult> RescheduleAsync(
            [AuthorMustOwn] SbuReminder reminder,
            DateTime? newTimestamp = null
        )
        {
            if (newTimestamp is null)
            {
                await Reply("When do you want to be reminded.");

                TypeParser<DateTime> parser = Context.Bot.Commands.GetTypeParser<DateTime>();
                MessageReceivedEventArgs? waitResult;

                await using (Context.BeginYield())
                {
                    waitResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                }

                if (waitResult is null)
                    return Reply("Aborted: You did not provide a timestamp.");

                TypeParserResult<DateTime>? parseResult = await parser.ParseAsync(
                    null,
                    waitResult.Message.Content,
                    Context
                );

                if (!parseResult.IsSuccessful)
                    return Reply($"Aborted: {parseResult.FailureReason}.");

                newTimestamp = parseResult.Value;
            }

            if (newTimestamp + TimeSpan.FromMilliseconds(500) >= DateTimeOffset.Now)
            {
                await Context.Services.GetRequiredService<ReminderService>()
                    .RescheduleReminderAsync(reminder.Id, newTimestamp.Value);
            }

            return Reply(
                new LocalEmbed()
                    .WithTitle("Reminder Rescheduled")
                    .WithDescription(reminder.Message)
                    .WithFooter("Due")
                    .WithTimestamp(newTimestamp)
            );
        }

        [Command("cancel", "remove", "delete")]
        [Description("Cancels the given reminder.")]
        public async Task<DiscordCommandResult> CancelAsync([AuthorMustOwn] SbuReminder reminder)
        {
            await Context.Services.GetRequiredService<ReminderService>().UnscheduledReminderAsync(reminder.Id);

            return Reply(
                new LocalEmbed()
                    .WithTitle("Reminder Cancelled")
                    .WithDescription(reminder.Message)
                    .WithFooter("Cancelled")
                    .WithCurrentTimestamp()
            );
        }

        [Command("list")]
        [Description("Lists the given reminder, or all if no reminder is specified.")]
        public DiscordCommandResult List(
            [OverrideDefault("all"), AuthorMustOwn]
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

            if (Context.Services.GetRequiredService<ReminderService>().CurrentReminders is not { Count: > 0 } reminders)
                return Reply("You have no reminders.");

            return MaybePages(
                reminders.Values
                    .Where(r => r.OwnerId == Context.Author.Id)
                    .Select(
                        r => $"[`{r.Id}`]({r.JumpUrl})\n{r.DueAt}\n"
                            + $"{(r.Message is { } ? $"\"{r.Message}\"" : "No Message")}\n"
                    ),
                "Your Reminders"
            );
        }
    }
}