using System;
using System.Threading.Tasks;

using Disqord;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class ColorRoleTypeParser : SbuTypeParserBase<SbuColorRole>
    {
        protected override async ValueTask<TypeParserResult<SbuColorRole>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        )
        {
            SbuColorRole? role = null;
            SbuGuild guild = await context.GetOrCreateGuildAsync();

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();
            TypeParser<IRole> roleParser = context.Bot.Commands.GetTypeParser<IRole>();

            if (await guidParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } guidParseResult)
            {
                await using (context.BeginYield())
                {
                    role = await context.Db.ColorRoles.FirstOrDefaultAsync(
                        r => r.Id == guidParseResult.Value && r.GuildId == guild.Id,
                        context.Bot.StoppingToken
                    );
                }
            }
            else if (await roleParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } roleParseResult
                && SbuUtility.IsSbuColorRole(roleParseResult.Value)
            )
            {
                await using (context.BeginYield())
                {
                    role = await context.Db.ColorRoles.FirstOrDefaultAsync(
                        r => r.DiscordId == roleParseResult.Value.Id && r.GuildId == guild.Id,
                        context.Bot.StoppingToken
                    );
                }
            }

            return role is { }
                ? TypeParser<SbuColorRole>.Success(role)
                : TypeParser<SbuColorRole>.Failure("Could not find role.");
        }
    }
}