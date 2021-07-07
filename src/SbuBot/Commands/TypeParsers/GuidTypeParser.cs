using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands
{
    public sealed class GuidTypeParser : DiscordTypeParser<Guid>
    {
        public override ValueTask<TypeParserResult<Guid>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordCommandContext context
        ) => Guid.TryParse(value, out Guid guid)
            ? Success(guid)
            : Failure("Could not parse id.");
    }
}