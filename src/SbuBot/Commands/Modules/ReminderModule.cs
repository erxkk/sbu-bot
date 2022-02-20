using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

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
        public async Task<DiscordCommandResult> ListAsync(
            [AuthorMustOwn]
            [Description("The reminder that should be listed.")]
            [Remarks("Lists all reminders if none is specified.")]
            SbuReminder? reminder = null
        )
        {
            if (reminder is { })
            {
                return Response(
                    new LocalEmbed()
                        .WithTitle("Reminder")
                        .WithDescription(reminder.Message)
                        .WithFooter(reminder.GetFormattedId())
                        .WithTimestamp(reminder.DueAt)
                );
            }

            if (await Context.Services.GetRequiredService<ReminderService>()
                    .FetchRemindersAsync(
                        query => query.Where(r => r.OwnerId == Context.Author.Id && r.GuildId == Context.GuildId)
                    )
                is not { Count: > 0 } reminders)
                return Reply("You have no reminders.");

            return DistributedPages(
                reminders.Values.Select(
                    r => string.Format(
                        "{0} {1} {2}\n{3}\n",
                        SbuGlobals.BULLET,
                        Markdown.Link(r.GetFormattedId(), r.GetJumpUrl()),
                        Markdown.Timestamp(r.DueAt),
                        r.Message is { } ? $"{SbuUtility.Truncate(r.Message, 256)}" : "`No Message`"
                    )
                ),
                embedFactory: embed => embed.WithTitle("Your Reminders")
            );
        }
    }
}
