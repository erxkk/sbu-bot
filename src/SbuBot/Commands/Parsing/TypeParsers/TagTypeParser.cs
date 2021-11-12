using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class TagTypeParser : DiscordGuildTypeParser<SbuTag>
    {
        public override async ValueTask<TypeParserResult<SbuTag>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        ) => await context.GetTagAsync(value) is { } tag
            ? Success(tag)
            : Failure("Could not find tag.");
    }
}
