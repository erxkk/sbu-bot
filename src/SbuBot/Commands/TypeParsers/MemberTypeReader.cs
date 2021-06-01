using System;
using System.Threading.Tasks;

using Disqord;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class MemberTypeReader : SbuTypeParser<SbuMember>
    {
        protected override async ValueTask<TypeParserResult<SbuMember>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        )
        {
            SbuMember? member = null;

            TypeParser<Guid> guidParser = context.Bot.Commands.GetTypeParser<Guid>();
            TypeParser<IMember> roleParser = context.Bot.Commands.GetTypeParser<IMember>();

            if (await guidParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } guidParseResult)
            {
                await using (context.BeginYield())
                {
                    member = await context.Db.Members.FirstOrDefaultAsync(t => t.Id == guidParseResult.Value);
                }
            }
            else if (await roleParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } roleParseResult)
            {
                await using (context.BeginYield())
                {
                    member = await context.Db.Members.FirstOrDefaultAsync(
                        t => t.DiscordId == roleParseResult.Value.Id
                    );
                }
            }

            return member is { }
                ? TypeParser<SbuMember>.Success(member)
                : TypeParser<SbuMember>.Failure("Could not find member.");
        }
    }
}