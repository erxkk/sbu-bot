using System;
using System.Threading.Tasks;

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
    public sealed partial class ReminderModule
    {
        [Command("edit")]
        [Description("Reschedules the given reminder.")]
        [Usage("reminder edit last in 2 days", "reminder change 936DA01F in 5 seconds")]
        public async Task<DiscordCommandResult> EditAsync(
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
    }
}