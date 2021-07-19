using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class AutoResponseTypeParser : DiscordGuildTypeParser<SbuAutoResponse>
    {
        public override async ValueTask<TypeParserResult<SbuAutoResponse>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        )
        {
            SbuAutoResponse? autoResponse;

            await using (context.BeginYield())
            {
                autoResponse = await context.GetAutoResponseAsync(value);
            }

            return autoResponse is { } ? Success(autoResponse) : Failure("Could not find auto response.");
        }
    }
}