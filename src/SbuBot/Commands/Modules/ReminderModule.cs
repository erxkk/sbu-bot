using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Checks.Parameters;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("reminder", "remind", "remindme", "r"), RequireAuthorInDb]
    public sealed class ReminderModule : SbuModuleBase
    {
        [Command]
        public async Task<DiscordCommandResult> CreateReminderAsync(
            DateTime timestamp,
            string? message = null
        )
        {
            var newReminder = new SbuReminder(Context, message, timestamp);

            await using (Context.BeginYield())
            {
                await Context.Services.GetRequiredService<ReminderService>().ScheduleReminderAsync(newReminder);
            }

            return Reply(
                new LocalEmbedBuilder()
                    .WithTitle("Reminder Scheduled")
                    .WithDescription(message)
                    .WithFooter("Due at")
                    .WithTimestamp(newReminder.DueAt)
            );
        }

        [Command]
        public async Task<DiscordCommandResult> CreateReminderInteractiveAsync(string message)
        {
            TypeParser<DateTime> parser = Context.Bot.Commands.GetTypeParser<DateTime>();
            MessageReceivedEventArgs? waitResult;

            await Reply("Please provide a timestamp next.");

            await using (Context.BeginYield())
            {
                waitResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
            }

            if (waitResult is null)
                return Reply("Aborted, you did not provide a timestamp.");

            TypeParserResult<DateTime>? parseResult = await parser.ParseAsync(
                null,
                waitResult.Message.Content,
                Context
            );

            if (!parseResult.IsSuccessful)
                return Reply($"Aborted, reason: \"{parseResult.FailureReason}\".");

            var newReminder = new SbuReminder(Context, message, parseResult.Value);

            await using (Context.BeginYield())
            {
                await Context.Services.GetRequiredService<ReminderService>().ScheduleReminderAsync(newReminder);
            }

            return Reply(
                new LocalEmbedBuilder()
                    .WithTitle("Reminder Scheduled")
                    .WithDescription(message)
                    .WithFooter("Due at")
                    .WithTimestamp(newReminder.DueAt)
            );
        }

        [Command("edit", "reschedule", "change")]
        public async Task<DiscordCommandResult> RescheduleReminderAsync(
            [AuthorMustOwn] SbuReminder reminder,
            DateTime? newTimestamp = null
        )
        {
            if (newTimestamp is null)
            {
                await Reply("Please provide a timestamp next.");

                TypeParser<DateTime> parser = Context.Bot.Commands.GetTypeParser<DateTime>();
                MessageReceivedEventArgs? waitResult;

                await using (Context.BeginYield())
                {
                    waitResult = await Context.WaitForMessageAsync(e => e.Member.Id == Context.Author.Id);
                }

                if (waitResult is null)
                    return Reply("Aborted, you did not provide a timestamp.");

                TypeParserResult<DateTime>? parseResult = await parser.ParseAsync(
                    null,
                    waitResult.Message.Content,
                    Context
                );

                if (!parseResult.IsSuccessful)
                    return Reply($"Aborted, reason: \"{parseResult.FailureReason}\".");

                newTimestamp = parseResult.Value;
            }

            if (newTimestamp + TimeSpan.FromMilliseconds(500) >= DateTimeOffset.Now)
            {
                await using (Context.BeginYield())
                {
                    await Context.Services.GetRequiredService<ReminderService>()
                        .RescheduleReminderAsync(reminder.Id, newTimestamp.Value);
                }
            }

            return Reply(
                new LocalEmbedBuilder()
                    .WithTitle("Reminder Rescheduled")
                    .WithDescription(reminder.Message)
                    .WithFooter("Due at")
                    .WithTimestamp(newTimestamp)
            );
        }

        [Command("remove", "unschedule", "delete", "cancel")]
        public async Task<DiscordCommandResult> UnscheduleReminderAsync([AuthorMustOwn] SbuReminder reminder)
        {
            await using (Context.BeginYield())
            {
                await Context.Services.GetRequiredService<ReminderService>().UnscheduledReminderAsync(reminder.Id);
            }

            return Reply(
                new LocalEmbedBuilder()
                    .WithTitle("Reminder Unscheduled")
                    .WithDescription(reminder.Message)
                    .WithFooter("Cancelled at")
                    .WithCurrentTimestamp()
            );
        }

        [Command("list", "show", "l")]
        public DiscordCommandResult ListReminders([AuthorMustOwn] SbuReminder? reminder = null) => reminder is { }
            ? Reply(
                new LocalEmbedBuilder()
                    .WithTitle("Reminder")
                    .WithDescription(reminder.Message)
                    .WithFooter("Due at")
                    .WithTimestamp(reminder.DueAt)
            )
            : MaybePages(
                Context.Services.GetRequiredService<ReminderService>()
                    .CurrentReminders.Values
                    .Where(r => r.OwnerId == Context.Author.Id)
                    .Select(
                        r => $"{r.DueAt}[`{r.Id}`]({r.JumpUrl})\n"
                            + $"{(r.Message is { } ? $"\"{r.Message}\"" : "No Message")}\n"
                    ),
                "Your Reminders"
            );
    }
}