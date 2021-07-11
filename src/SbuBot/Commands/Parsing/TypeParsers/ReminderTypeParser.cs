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
            IReadOnlyDictionary<Snowflake, SbuReminder> reminders = await service.GetRemindersAsync();

            if (value.Equals("last", StringComparison.OrdinalIgnoreCase))
            {
                return reminders
                    .Values.FirstOrDefault(
                        r => r.OwnerId == context.Author.Id && r.GuildId == context.GuildId
                    ) is { } queriedReminder
                    ? Success(queriedReminder)
                    : Failure("Could not find reminder.");
            }

            TypeParser<Snowflake> guidParser = context.Bot.Commands.GetTypeParser<Snowflake>();

            if (await guidParser.ParseAsync(parameter, value, context) is not { IsSuccessful: true } guidParseResult)
                return Failure("Could not parse reminder.");

            return reminders.TryGetValue(guidParseResult.Value, out var indexedReminder)
                ? Success(indexedReminder)
                : Failure("Could not find reminder.");
        }
    }
}