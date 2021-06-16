using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

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
            SbuGuild guild = await context.GetSbuDbContext().GetSbuGuildAsync(context.Guild);

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();

            if (await guidParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } guidParseResult)
            {
                await using (context.BeginYield())
                {
                    tag = await context.GetSbuDbContext()
                        .Tags
                        .FirstOrDefaultAsync(
                            t => t.Id == guidParseResult.Value && t.GuildId == guild.Id,
                            context.Bot.StoppingToken
                        );
                }
            }
            else
            {
                await using (context.BeginYield())
                {
                    tag = await context.GetSbuDbContext()
                        .Tags
                        .FirstOrDefaultAsync(
                            t => t.Name == value && t.GuildId == guild.Id,
                            context.Bot.StoppingToken
                        );
                }
            }

            return tag is { } ? TypeParser<SbuTag>.Success(tag) : TypeParser<SbuTag>.Failure("Could not find tag.");
        }
    }
}