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
            TypeParser<IMember> memberParser = context.Bot.Commands.GetTypeParser<IMember>();

            if (await memberParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } memberParseResult)
            {
                await using (context.BeginYield())
                {
                    member = await context.GetSbuDbContext()
                        .GetMemberAsync(
                            memberParseResult.Value.Id,
                            context.GuildId,
                            members => members.Include(m => m.ColorRole),
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