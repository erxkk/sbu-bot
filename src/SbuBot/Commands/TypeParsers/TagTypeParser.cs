using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class TagTypeParser : SbuTypeParserBase<SbuTag>
    {
        protected override async ValueTask<TypeParserResult<SbuTag>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        )
        {
            SbuTag? tag;

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();

            if (await guidParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } guidParseResult)
            {
                await using (context.BeginYield())
                {
                    tag = await context.Db.Tags.FirstOrDefaultAsync(t => t.Id == guidParseResult.Value, context.Bot.StoppingToken);
                }
            }
            else
            {
                await using (context.BeginYield())
                {
                    tag = await context.Db.Tags.FirstOrDefaultAsync(t => t.Name == value, context.Bot.StoppingToken);
                }
            }

            return tag is { } ? TypeParser<SbuTag>.Success(tag) : TypeParser<SbuTag>.Failure("Could not find tag.");
        }
    }
}