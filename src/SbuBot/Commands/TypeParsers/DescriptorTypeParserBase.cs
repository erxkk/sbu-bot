using System.Linq;
using System.Threading.Tasks;

using Qmmands;

using SbuBot.Commands.Descriptors;

namespace SbuBot.Commands.TypeParsers
{
    public abstract class DescriptorTypeParserBase<T> : SbuTypeParserBase<T>
        where T : struct, IDescriptor
    {
        protected abstract ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string[] values,
            SbuCommandContext context
        );

        // TODO: ignore escaped separator
        protected sealed override ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        ) => ParseAsync(
            parameter,
            value.Split(SbuBotGlobals.DESCRIPTOR_SEPARATOR).Select(s => s.Trim()).ToArray(),
            context
        );
    }
}