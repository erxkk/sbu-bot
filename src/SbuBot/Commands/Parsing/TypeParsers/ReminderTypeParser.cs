using System;
using System.Collections.Generic;
using System.Globalization;
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
            // BUG: exception in some task on aggregate accessor?
            // see: log20220219.txt [18:49]
            // this might not fix the issue but expose it in the logs next time it happens
            ReminderService service = context.Services.GetRequiredService<ReminderService>();

            if (value.Equals("last", StringComparison.OrdinalIgnoreCase))
            {
                return await service.FetchReminderAsync(
                    query => query
                        .OrderByDescending(r => r.CreatedAt)
                        .Where(r => r.GuildId == context.GuildId && r.OwnerId == context.Author.Id)
                ) is { } queriedReminder
                    ? Success(queriedReminder)
                    : Failure("Could not find reminder.");
            }

            TypeParser<Snowflake> snowflakeParser = context.Bot.Commands.GetTypeParser<Snowflake>();

            // in rare cases this can fail if all digits in the hex string are valid decimal
            if (await snowflakeParser.ParseAsync(parameter, value, context)
                is { IsSuccessful: true } snowflakeParseResult)
            {
                return await service.FetchReminderAsync(
                    query => query.Where(r => r.MessageId == snowflakeParseResult.Value)
                ) is { } reminder
                    ? Success(reminder)
                    : Failure("Could not find reminder.");
            }

            if (ulong.TryParse(value, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out ulong ulongId))
            {
                return await service.FetchReminderAsync(query => query.Where(r => r.MessageId == ulongId))
                    is { } reminder
                    ? Success(reminder)
                    : Failure("Could not find reminder.");
            }

            return Failure("Could not parse reminder.");
        }
    }
}
