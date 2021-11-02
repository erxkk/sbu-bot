using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Parsing;
using SbuBot.Commands.Views;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ReminderModule
    {
        [Command("delete")]
        [Description("Cancels the given reminder.")]
        [Usage("reminder remove last", "reminder delete 936DA01F", "reminder cancel all")]
        public async Task<DiscordCommandResult> DeleteAsync(
            [AuthorMustOwn][Description("The reminder that should be canceled.")]
            OneOrAll<SbuReminder> reminder
        )
        {
            if (reminder.IsAll)
            {
                ConfirmationState confirmationResult = await ConfirmationAsync(
                    "Reminder Removal",
                    "Are you sure you want to cancel all your reminders?"
                );

                switch (confirmationResult)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                IReadOnlyList<SbuReminder> cancelled = await Context.Services.GetRequiredService<ReminderService>()
                    .CancelAsync(r => r.OwnerId == Context.Author.Id);

                return Reply(
                    new LocalEmbed()
                        .WithTitle("Cancelled all reminders.")
                        .WithDescription(
                            cancelled.Select(r => $"{SbuGlobals.BULLET} `{r.MessageId.RawValue:X}`").ToNewLines()
                        )
                        .WithFooter("Cancelled")
                        .WithCurrentTimestamp()
                );
            }
            else
            {
                ConfirmationState confirmationResult = await ConfirmationAsync(
                    "Reminder Removal",
                    $"Are you sure you want to cancel `{reminder.Value.MessageId.RawValue:X}`?"
                );

                switch (confirmationResult)
                {
                    case ConfirmationState.None:
                    case ConfirmationState.Aborted:
                    case ConfirmationState.TimedOut:
                        return Reply("Aborted.");

                    case ConfirmationState.Confirmed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await Context.Services.GetRequiredService<ReminderService>().CancelAsync(reminder.Value.MessageId);

                return Reply(
                    new LocalEmbed()
                        .WithTitle("Reminder Cancelled")
                        .WithDescription($"`{reminder.Value.MessageId.RawValue:X}`\n{reminder.Value.Message}")
                        .WithFooter("Cancelled")
                        .WithCurrentTimestamp()
                );
            }
        }
    }
}