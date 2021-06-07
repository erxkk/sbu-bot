using System;
using System.Threading.Tasks;

using Qmmands;

using SbuBot.Commands.TypeParsers;

namespace SbuBot.Commands
{
    public sealed class GuidTypeParser : SbuTypeParserBase<Guid>
    {
        protected override ValueTask<TypeParserResult<Guid>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        ) => Guid.TryParse(value, out Guid guid)
            ? TypeParser<Guid>.Success(guid)
            : TypeParser<Guid>.Failure("Could not parse id.");
    }
}