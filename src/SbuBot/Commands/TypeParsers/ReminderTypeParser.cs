using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class ReminderTypeParser : DiscordGuildTypeParser<SbuReminder>
    {
        public override async ValueTask<TypeParserResult<SbuReminder>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        )
        {
            // TODO: fetch reminders form db if not production as they are not loaded on start up or load with meaningless dispatch
            ReminderService service = context.Services.GetRequiredService<ReminderService>();

            if (value.Equals("last", StringComparison.OrdinalIgnoreCase))
            {
                return service.GetCurrentReminders()
                    .Values.FirstOrDefault(
                        r => r.OwnerId == context.Author.Id && r.GuildId == context.GuildId
                    ) is { } queriedReminder
                    ? Success(queriedReminder)
                    : Failure("Could not find reminder.");
            }

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();

            if (await guidParser.ParseAsync(parameter, value, context) is not { IsSuccessful: true } guidParseResult)
                return Failure("Could not parse reminder.");

            return service.GetCurrentReminders().TryGetValue(guidParseResult.Value, out var indexedReminder)
                ? Success(indexedReminder)
                : Failure("Could not find reminder.");
        }
    }
}