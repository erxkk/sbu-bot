using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.TypeParsers.Descriptors;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("reminder", "remind", "remindme")]
    [Description("A collection of commands for creating modifying and removing reminders.")]
    [Remarks("Reminder timestamps may be given as human readable timespans or strictly colon `:` separated integers.")]
    public sealed class ReminderModule : SbuModuleBase
    {
        [Group("create", "make", "new")]
        [Description("Creates a new reminder with the given timestamp and optional message.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> CreateAsync(
                [Description("The reminder descriptor.")]
                ReminderDescriptor reminderDescriptor
            )
            {
                SbuMember owner = await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author);
                SbuGuild guild = await Context.GetSbuDbContext().GetSbuGuildAsync(Context.Guild);

                SbuReminder newReminder = new(
                    Context,
                    owner.Id,
                    guild.Id,
                    reminderDescriptor.Message,
                    reminderDescriptor.Timestamp
                );

                await Context.Services.GetRequiredService<ReminderService>()
                    .ScheduleAsync(newReminder, cancellationToken: Context.Bot.StoppingToken);

                return Reply(
                    new LocalEmbed()
                        .WithTitle("Reminder Scheduled")
                        .WithDescription(reminderDescriptor.Message)
                        .WithFooter("Due")
                        .WithTimestamp(newReminder.DueAt)
                );
            }

            [Command]
            public async Task<DiscordCommandResult> CreateInteractiveAsync(
                [Maximum(SbuReminder.MAX_MESSAGE_LENGTH)][Description("The optional message of the reminder.")]
                string? message = null
            )
            {
                TypeParser<DateTime> parser = Context.Bot.Commands.GetTypeParser<DateTime>();

                await Reply("When do you want to be reminded?");

                MessageReceivedEventArgs? waitResult;

                await using (Context.BeginYield())
                {
                    waitResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
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

                SbuMember owner = await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author);
                SbuGuild guild = await Context.GetSbuDbContext().GetSbuGuildAsync(Context.Guild);
                SbuReminder newReminder = new(Context, owner.Id, guild.Id, message, parseResult.Value);

                await using (Context.BeginYield())
                {
                    await Context.Services.GetRequiredService<ReminderService>()
                        .ScheduleAsync(newReminder, cancellationToken: Context.Bot.StoppingToken);
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
            [AuthorMustOwn][Description("The reminder to reschedule.")]
            SbuReminder reminder,
            [MustBeFuture][Description("The new timestamp.")]
            DateTime newTimestamp
        )
        {
            if (newTimestamp + TimeSpan.FromMilliseconds(500) >= DateTimeOffset.Now)
            {
                await Context.Services.GetRequiredService<ReminderService>()
                    .RescheduleAsync(reminder.Id, newTimestamp, Context.Bot.StoppingToken);
            }

            return Reply(
                new LocalEmbed()
                    .WithTitle("Reminder Rescheduled")
                    .WithDescription(reminder.Message)
                    .WithFooter("Due")
                    .WithTimestamp(newTimestamp)
            );
        }

        [Group("remove", "delete", "cancel")]
        [Description("A group of commands for cancelling reminders.")]
        public sealed class RemoveGroup : SbuModuleBase
        {
            [Command]
            [Description("Cancels the given reminder.")]
            public async Task<DiscordCommandResult> CancelAsync(
                [AuthorMustOwn][Description("The reminder that should be canceled.")]
                SbuReminder reminder
            )
            {
                await Context.Services.GetRequiredService<ReminderService>().CancelAsync(reminder.Id);

                return Reply(
                    new LocalEmbed()
                        .WithTitle("Reminder Cancelled")
                        .WithDescription(reminder.Message)
                        .WithFooter("Cancelled")
                        .WithCurrentTimestamp()
                );
            }

            [Command("all")]
            [Description("Cancels all of the command author's reminders.")]
            public async Task<DiscordCommandResult> CancelAllAsync()
            {
                await Reply("Are yous ure you want to cancel all your reminders? Respond `yes` to confirm.");

                MessageReceivedEventArgs? waitResult;

                await using (Context.BeginYield())
                {
                    waitResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
                }

                if (waitResult is null || !waitResult.Message.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    return Reply("Aborted.");

                SbuMember owner = await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author);

                await Context.Services.GetRequiredService<ReminderService>()
                    .CancelAsync(q => q.Where(r => r.Value.OwnerId == owner.Id));

                return Reply("Cancelled all reminders.");
            }
        }

        [Group("list")]
        [Description("A group of commands for listing reminders.")]
        public sealed class ListGroup : SbuModuleBase
        {
            [Command]
            [Description("Lists the given reminder.")]
            public DiscordCommandResult List(
                [AuthorMustOwn][Description("The reminder that should be listed.")]
                SbuReminder reminder
            ) => Reply(
                new LocalEmbed()
                    .WithTitle("Reminder")
                    .WithDescription(reminder.Message)
                    .WithFooter("Due")
                    .WithTimestamp(reminder.DueAt)
            );

            [Command("all")]
            [Description("Lists all of the command author's reminders.")]
            public async Task<DiscordCommandResult> ListAllAsync()
            {
                if (Context.Services.GetRequiredService<ReminderService>().CurrentReminders
                    is not { Count: > 0 } reminders)
                    return Reply("You have no reminders.");

                SbuMember owner = await Context.GetSbuDbContext().GetSbuMemberAsync(Context.Author);

                return FilledPages(
                    reminders.Values
                        .Where(r => r.OwnerId == owner.Id)
                        .Select(
                            r => $"[`{r.Id}`]({r.JumpUrl})\n{r.DueAt}\n"
                                + $"{(r.Message is { } ? $"\"{r.Message}\"" : "No Message")}\n"
                        ),
                    embedModifier: embed => embed.WithTitle("Your Reminders")
                );
            }
        }
    }
}