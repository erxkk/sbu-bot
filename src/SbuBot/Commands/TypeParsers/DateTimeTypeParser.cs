using System;
using System.Threading.Tasks;

using HumanTimeParser.Core.Parsing;
using HumanTimeParser.English;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class DateTimeTypeParser : SbuTypeParserBase<DateTime>
    {
        protected override ValueTask<TypeParserResult<DateTime>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        )
        {
            EnglishTimeParser timeParser = context.Services.GetRequiredService<EnglishTimeParser>();

            if (timeParser.Parse(value) is ISuccessfulTimeParsingResult<DateTime> result)
                return TypeParser<DateTime>.Success(result.Value);

            return TypeParser<DateTime>.Failure("Could not parse timestamp.");
        }
    }
}