using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class UriTypeParser : DiscordTypeParser<Uri>
    {
        public override ValueTask<TypeParserResult<Uri>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordCommandContext context
        ) => Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri? uri)
            ? Success(uri)
            : Failure("Could not parse url.");
    }
}