using System;
using System.Threading.Tasks;

using Disqord.Bot;

using HumanTimeParser.Core.Parsing;
using HumanTimeParser.English;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class TimeSpanTypeParser : DiscordTypeParser<TimeSpan>
    {
        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordCommandContext context
        )
        {
            EnglishTimeParser timeParser = context.Services.GetRequiredService<EnglishTimeParser>();

            try
            {
                if (timeParser.Parse(value) is ISuccessfulTimeParsingResult<DateTime> result)
                    return Success(result.Value - DateTime.Now);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Failure($"The parsed timespan was out of range: `{ex.Message}`.");
            }

            return Failure("Could not parse timespan.");
        }
    }
}
