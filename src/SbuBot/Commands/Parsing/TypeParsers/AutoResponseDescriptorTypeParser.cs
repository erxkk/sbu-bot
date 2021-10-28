using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Models;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class AutoResponseDescriptorTypeParser : DescriptorTypeParserBase<AutoResponseDescriptor>
    {
        protected override ValueTask<TypeParserResult<AutoResponseDescriptor>> ParseAsync(
            Parameter parameter,
            string[] values,
            DiscordGuildCommandContext context
        )
        {
            if (values.Length != 2)
                return Failure($"Two parts were expected, found {values.Length}.");

            switch (SbuAutoResponse.IsValidTrigger(values[0]))
            {
                case SbuAutoResponse.ValidTriggerType.TooLong:
                    return Failure(
                        $"The auto response trigger can be at most {SbuTag.MAX_NAME_LENGTH} characters long."
                    );

                case SbuAutoResponse.ValidTriggerType.Reserved:
                    return Failure("The auto response trigger cannot be a reserved keyword.");

                case SbuAutoResponse.ValidTriggerType.Valid:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (SbuAutoResponse.IsValidResponse(values[1]))
            {
                case SbuAutoResponse.ValidResponseType.TooLong:
                    return Failure(
                        $"The auto response can be at most {SbuTag.MAX_NAME_LENGTH} characters long."
                    );

                case SbuAutoResponse.ValidResponseType.Valid:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Success(new() { Trigger = values[0], Response = values[1] });
        }
    }
}