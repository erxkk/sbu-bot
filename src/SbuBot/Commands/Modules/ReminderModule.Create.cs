using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Kkommon;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ReminderModule
    {
        [Group("create", "me")]
        [Description("Creates a new reminder with the given timestamp and optional message.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            [UsageOverride(
                "reminder create in 3 days :: do the thing",
                "reminder make tomorrow at 15:00 :: do the thing",
                "remind me in 3 days :: do the thing"
            )]
            public async Task<DiscordCommandResult> CreateAsync(
                [Description("The reminder descriptor `<timestamp> :: <text>`.")]
                ReminderDescriptor descriptor
            )
            {
                SbuReminder newReminder = new(
                    Context,
                    Context.Author.Id,
                    Context.Guild.Id,
                    descriptor.Message,
                    descriptor.Timestamp
                );

                await Context.Services.GetRequiredService<ReminderService>().ScheduleAsync(newReminder);

                return Response(
                    new LocalEmbed()
                        .WithTitle("Reminder Scheduled")
                        .WithFooter(newReminder.GetFormattedId())
                        .WithTimestamp(newReminder.DueAt)
                );
            }

            [Command]
            public async Task<DiscordCommandResult> CreateNoMessageAsync(
                [Description("The time at which to remind you.")]
                DateTime timestamp
            )
            {
                SbuReminder newReminder = new(Context, Context.Author.Id, Context.Guild.Id, null, timestamp);

                await Context.Services.GetRequiredService<ReminderService>().ScheduleAsync(newReminder);

                return Response(
                    new LocalEmbed()
                        .WithTitle("Reminder Scheduled")
                        .WithFooter(newReminder.GetFormattedId())
                        .WithTimestamp(newReminder.DueAt)
                );
            }
        }
    }
}
