using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Models;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class TagDescriptorTypeParser : DescriptorTypeParserBase<TagDescriptor>
    {
        protected override ValueTask<TypeParserResult<TagDescriptor>> ParseAsync(
            Parameter parameter,
            string[] values,
            DiscordGuildCommandContext context
        )
        {
            if (values.Length != 2)
                return Failure($"Two parts were expected, found {values.Length}.");

            switch (SbuTag.IsValidTagName(values[0]))
            {
                case SbuTag.ValidNameType.TooShort:
                    return Failure(
                        $"The tag name must be at least {SbuTag.MIN_NAME_LENGTH} characters long."
                    );

                case SbuTag.ValidNameType.TooLong:
                    return Failure(
                        $"The tag name must be at most {SbuTag.MAX_NAME_LENGTH} characters long."
                    );

                case SbuTag.ValidNameType.Reserved:
                    return Failure("The tag name cannot be a reserved keyword.");

                case SbuTag.ValidNameType.Valid:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (values[1].Length > SbuTag.MAX_CONTENT_LENGTH)
            {
                return Failure(
                    $"The tag content must be at most {SbuTag.MAX_CONTENT_LENGTH} characters long."
                );
            }

            return Success(new() { Name = values[0], Content = values[1] });
        }
    }
}