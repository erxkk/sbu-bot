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
    public sealed class MemberTypeReader : DiscordGuildTypeParser<SbuMember>
    {
        public override async ValueTask<TypeParserResult<SbuMember>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        )
        {
            SbuMember? member = null;

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();
            TypeParser<IMember> roleParser = context.Bot.Commands.GetTypeParser<IMember>();

            if (await guidParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } guidParseResult)
            {
                await using (context.BeginYield())
                {
                    member = await context.GetSbuDbContext().Members.FirstOrDefaultAsync(
                        t => t.Id == guidParseResult.Value,
                        context.Bot.StoppingToken
                    );
                }
            }
            else if (await roleParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } roleParseResult)
            {
                await using (context.BeginYield())
                {
                    member = await context.GetSbuDbContext().Members.FirstOrDefaultAsync(
                        t => t.DiscordId == roleParseResult.Value.Id,
                        context.Bot.StoppingToken
                    );
                }
            }

            return member is { }
                ? TypeParser<SbuMember>.Success(member)
                : TypeParser<SbuMember>.Failure("Could not find member.");
        }
    }
}