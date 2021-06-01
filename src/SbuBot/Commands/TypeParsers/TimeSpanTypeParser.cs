using System;
using System.Threading.Tasks;

using HumanTimeParser.Core.Parsing;
using HumanTimeParser.English;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class TimeSpanTypeParser : SbuTypeParser<TimeSpan>
    {
        protected override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        )
        {
            EnglishTimeParser timeParser = context.Services.GetRequiredService<EnglishTimeParser>();

            if (timeParser.Parse(value) is ISuccessfulTimeParsingResult<DateTime> result)
                return TypeParser<TimeSpan>.Success(result.Value - DateTime.Now);

            return TypeParser<TimeSpan>.Failure("Could not parse timespan.");
        }
    }
}