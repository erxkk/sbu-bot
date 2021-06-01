using System;
using System.Threading.Tasks;

using Disqord;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class ColorRoleTypeParser : SbuTypeParser<SbuColorRole>
    {
        protected override async ValueTask<TypeParserResult<SbuColorRole>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        )
        {
            SbuColorRole? role = null;

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();
            TypeParser<IRole> roleParser = context.Bot.Commands.GetTypeParser<IRole>();

            if (await guidParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } guidParseResult)
            {
                await using (context.BeginYield())
                {
                    role = await context.Db.ColorRoles.FirstOrDefaultAsync(t => t.Id == guidParseResult.Value);
                }
            }
            else if (await roleParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } roleParseResult
                && Utility.IsSbuColorRole(roleParseResult.Value)
            )
            {
                await using (context.BeginYield())
                {
                    role = await context.Db.ColorRoles.FirstOrDefaultAsync(
                        t => t.DiscordId == roleParseResult.Value.Id
                    );
                }
            }

            return role is { }
                ? TypeParser<SbuColorRole>.Success(role)
                : TypeParser<SbuColorRole>.Failure("Could not find tag.");
        }
    }
}