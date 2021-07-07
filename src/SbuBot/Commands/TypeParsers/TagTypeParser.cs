using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class TagTypeParser : DiscordGuildTypeParser<SbuTag>
    {
        public override async ValueTask<TypeParserResult<SbuTag>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        )
        {
            SbuTag? tag;

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();

            if (await guidParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } guidParseResult)
            {
                await using (context.BeginYield())
                {
                    tag = await context.GetTagAsync(guidParseResult.Value);
                }
            }
            else
            {
                await using (context.BeginYield())
                {
                    tag = await context.GetTagAsync(value);
                }
            }

            return tag is { } ? Success(tag) : Failure("Could not find tag.");
        }
    }
}