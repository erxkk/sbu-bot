using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class ColorRoleTypeParser : DiscordGuildTypeParser<SbuColorRole>
    {
        public override async ValueTask<TypeParserResult<SbuColorRole>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        )
        {
            SbuColorRole? role = null;
            SbuGuild guild = await context.GetSbuDbContext().GetSbuGuildAsync(context.Guild);

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();
            TypeParser<IRole> roleParser = context.Bot.Commands.GetTypeParser<IRole>();

            if (await guidParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } guidParseResult)
            {
                await using (context.BeginYield())
                {
                    role = await context.GetSbuDbContext()
                        .ColorRoles.FirstOrDefaultAsync(
                            r => r.Id == guidParseResult.Value && r.GuildId == guild.Id,
                            context.Bot.StoppingToken
                        );
                }
            }
            else if (await roleParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } roleParseResult
                && roleParseResult.Value.Position < context.Bot.GetColorRoleSeparator().Position
                && roleParseResult.Value.Color is { }
            )
            {
                await using (context.BeginYield())
                {
                    role = await context.GetSbuDbContext()
                        .ColorRoles.FirstOrDefaultAsync(
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