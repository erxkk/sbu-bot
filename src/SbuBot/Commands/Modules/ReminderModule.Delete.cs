using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Commands.Parsing.HelperTypes;
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

                return Response(
                    new LocalEmbed()
                        .WithTitle("Cancelled all reminders.")
                        .WithDescription(
                            cancelled.Select(
                                    r => string.Format(
                                        "{0} `{1}`",
                                        SbuGlobals.BULLET,
                                        Markdown.Link(r.GetFormattedId(), r.GetJumpUrl())
                                    )
                                )
                                .ToNewLines()
                        )
                        .WithCurrentTimestamp()
                );
            }
            else
            {
                ConfirmationState confirmationResult = await ConfirmationAsync(
                    "Reminder Removal",
                    $"Are you sure you want to cancel `{reminder.Value.GetFormattedId()}`?"
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

                return Response(
                    new LocalEmbed()
                        .WithTitle("Reminder Cancelled")
                        .WithDescription(
                            string.Format(
                                "{0}\n\n{1}",
                                reminder.Value.Message,
                                Markdown.Link("Original Message", reminder.Value.GetJumpUrl())
                            )
                        )
                        .WithFooter(reminder.Value.GetFormattedId())
                        .WithCurrentTimestamp()
                );
            }
        }
    }
}
