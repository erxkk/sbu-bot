using System;
using System.Threading.Tasks;

using Disqord.Bot;

using HumanTimeParser.Core.Parsing;
using HumanTimeParser.English;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class DateTimeTypeParser : DiscordTypeParser<DateTime>
    {
        public override ValueTask<TypeParserResult<DateTime>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordCommandContext context
        )
        {
            EnglishTimeParser timeParser = context.Services.GetRequiredService<EnglishTimeParser>();

            if (timeParser.Parse(value) is ISuccessfulTimeParsingResult<DateTime> result)
                return Success(result.Value);

            return Failure("Could not parse timestamp.");
        }
    }
}