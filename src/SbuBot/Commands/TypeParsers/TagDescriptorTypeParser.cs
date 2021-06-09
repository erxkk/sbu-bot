using System;
using System.Threading.Tasks;

using Qmmands;

using SbuBot.Commands.Descriptors;
using SbuBot.Models;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class TagDescriptorTypeParser : DescriptorTypeParserBase<TagDescriptor>
    {
        protected override ValueTask<TypeParserResult<TagDescriptor>> ParseAsync(
            Parameter parameter,
            string[] values,
            SbuCommandContext context
        )
        {
            if (values.Length != 2)
            {
                return TypeParser<TagDescriptor>.Failure(
                    $"One separator `{SbuGlobals.DESCRIPTOR_SEPARATOR}` is expected, found {values.Length}."
                );
            }

            switch (SbuTag.IsValidTagName(values[0]))
            {
                case SbuTag.ValidNameType.TooShort:
                    return TypeParser<TagDescriptor>.Failure(
                        $"The tag name must be at least {SbuTag.MIN_NAME_LENGTH} characters long."
                    );

                case SbuTag.ValidNameType.TooLong:
                    return TypeParser<TagDescriptor>.Failure(
                        $"The tag name must be at most {SbuTag.MAX_NAME_LENGTH} characters long."
                    );

                case SbuTag.ValidNameType.Reserved:
                    return TypeParser<TagDescriptor>.Failure("The tag name cannot be a reserved keyword.");

                case SbuTag.ValidNameType.Valid:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (values[1].Length > SbuTag.MAX_CONTENT_LENGTH)
            {
                return TypeParser<TagDescriptor>.Failure(
                    $"The tag content must be at most {SbuTag.MAX_CONTENT_LENGTH} characters long."
                );
            }

            return TypeParser<TagDescriptor>.Success(new() { Name = values[0], Content = values[1] });
        }
    }
}