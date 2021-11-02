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
                            "{0} [`{1:X}`]({2}) {3}\n{4}\n",
                            SbuGlobals.BULLET,
                            r.MessageId.RawValue,
                            r.JumpUrl,
                            Markdown.Timestamp(r.DueAt),
                            r.Message is { } ? $"`{r.Message}`" : "No Message"
                        )
                    ),
                embedFactory: embed => embed.WithTitle("Your Reminders")
            );
        }
    }
}