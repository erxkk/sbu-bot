using System;
using System.Threading.Tasks;

using HumanTimeParser.Core.Parsing;
using HumanTimeParser.English;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Descriptors;
using SbuBot.Models;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class ReminderDescriptorTypeParser : DescriptorTypeParserBase<ReminderDescriptor>
    {
        protected override ValueTask<TypeParserResult<ReminderDescriptor>> ParseAsync(
            Parameter parameter,
            string[] values,
            SbuCommandContext context
        )
        {
            if (values.Length != 2)
            {
                return TypeParser<ReminderDescriptor>.Failure(
                    $"One separator `{SbuGlobals.DESCRIPTOR_SEPARATOR}` is expected, found {values.Length}."
                );
            }

            if (values[2].Length > SbuReminder.MAX_MESSAGE_LENGTH)
            {
                return TypeParser<ReminderDescriptor>.Failure(
                    $"The given message must at most be {SbuReminder.MAX_MESSAGE_LENGTH} characters long."
                );
            }

            EnglishTimeParser timeParser = context.Services.GetRequiredService<EnglishTimeParser>();

            if (timeParser.Parse(values[0]) is not ISuccessfulTimeParsingResult<DateTime> result)
                return TypeParser<ReminderDescriptor>.Failure("Could not parse timestamp.");

            return result.Value > DateTimeOffset.Now
                ? TypeParser<ReminderDescriptor>.Success(new() { Timestamp = result.Value, Message = values[1] })
                : TypeParser<ReminderDescriptor>.Failure("The given timestamp must be in the future.");
        }
    }
}