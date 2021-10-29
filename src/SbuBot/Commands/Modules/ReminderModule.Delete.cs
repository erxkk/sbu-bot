using System;
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

                        default:
                            throw new ArgumentOutOfRangeException();
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

                        default:
                            throw new ArgumentOutOfRangeException();
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

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}