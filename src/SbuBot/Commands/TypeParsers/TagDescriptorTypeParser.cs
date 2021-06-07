using System.Threading.Tasks;

using Qmmands;

using SbuBot.Commands.Descriptors;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class TagDescriptorTypeParser : DescriptorTypeParserBase<TagDescriptor>
    {
        protected override ValueTask<TypeParserResult<TagDescriptor>> ParseAsync(
            Parameter parameter,
            string[] values,
            SbuCommandContext context
        ) => values.Length == 2
            ? TypeParser<TagDescriptor>.Success(new() { Name = values[0], Content = values[1] })
            : TypeParser<TagDescriptor>.Failure(
                $"One separator `{SbuBotGlobals.DESCRIPTOR_SEPARATOR}` is expected, found {values.Length}."
            );
    }
}