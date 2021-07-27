using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class ReminderTypeParser : DiscordGuildTypeParser<SbuReminder>
    {
        public override async ValueTask<TypeParserResult<SbuReminder>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        )
        {
            // TODO: parsing via hex number + display via hex number
            ReminderService service = context.Services.GetRequiredService<ReminderService>();
            IReadOnlyDictionary<Snowflake, SbuReminder> reminders = service.GetReminders();

            if (value.Equals("last", StringComparison.OrdinalIgnoreCase))
            {
                return reminders
                    .Values.FirstOrDefault(r => r.OwnerId == context.Author.Id) is { } queriedReminder
                    ? Success(queriedReminder)
                    : Failure("Could not find reminder.");
            }

            TypeParser<Snowflake> snowflakeParser = context.Bot.Commands.GetTypeParser<Snowflake>();

            if (await snowflakeParser.ParseAsync(parameter, value, context)
                is not { IsSuccessful: true } snowflakeParseResult)
                return Failure("Could not parse reminder.");

            return reminders.TryGetValue(snowflakeParseResult.Value, out var indexedReminder)
                ? Success(indexedReminder)
                : Failure("Could not find reminder.");
        }
    }
}