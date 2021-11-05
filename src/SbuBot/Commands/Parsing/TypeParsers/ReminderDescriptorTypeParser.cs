using System;
using System.Threading.Tasks;

using Disqord.Bot;

using HumanTimeParser.Core.Parsing;
using HumanTimeParser.English;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Models;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class ReminderDescriptorTypeParser : DescriptorTypeParserBase<ReminderDescriptor>
    {
        protected override ValueTask<TypeParserResult<ReminderDescriptor>> ParseAsync(
            Parameter parameter,
            string[] values,
            DiscordGuildCommandContext context
        )
        {
            if (values.Length != 2)
                return Failure($"Two parts were expected, found {values.Length}.");

            if (values[1].Length > SbuReminder.MAX_MESSAGE_LENGTH)
            {
                return Failure(
                    $"The given message must at most be {SbuReminder.MAX_MESSAGE_LENGTH} characters long."
                );
            }

            EnglishTimeParser timeParser = context.Services.GetRequiredService<EnglishTimeParser>();

            try
            {
                if (timeParser.Parse(values[0]) is not ISuccessfulTimeParsingResult<DateTime> result)
                    return Failure("Could not parse timestamp.");

                return result.Value > DateTimeOffset.Now
                    ? Success(new() { Timestamp = result.Value, Message = values[1] })
                    : Failure("The given timestamp must be in the future.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Failure($"The parsed timestamp was out of range: `{ex.Message}`.");
            }
        }
    }
}