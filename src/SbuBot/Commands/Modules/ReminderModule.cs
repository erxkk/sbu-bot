using System.Linq;

using Disqord;
using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("reminder", "remind")]
    [Description("A collection of commands for creating modifying and removing reminders.")]
    [Remarks("Reminder timestamps may be given as human readable timespans or strictly colon `:` separated integers.")]
    public sealed partial class ReminderModule : SbuModuleBase
    {
        [Command("list")]
        [Description("Lists the given reminder or all if non is given.")]
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
                        .WithFooter(reminder.GetFormattedId())
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
                            "{0} {1} {2}\n{3}\n",
                            SbuGlobals.BULLET,
                            Markdown.Link(r.GetFormattedId(), r.GetJumpUrl()),
                            Markdown.Timestamp(r.DueAt),
                            r.Message is { } ? $"{r.Message}" : "`No Message`"
                        )
                    ),
                embedFactory: embed => embed.WithTitle("Your Reminders")
            );
        }
    }
}