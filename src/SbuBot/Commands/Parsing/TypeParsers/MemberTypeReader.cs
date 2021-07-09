using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Parsing.TypeParsers
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

            if (value.Equals("me", StringComparison.OrdinalIgnoreCase))
            {
                return await context.GetMemberAsync(context.Author) is { } queriedMember
                    ? Success(queriedMember)
                    : Failure("Could not find reminder.");
            }

            TypeParser<IMember> memberParser = context.Bot.Commands.GetTypeParser<IMember>();

            if (await memberParser.ParseAsync(parameter, value, context) is { IsSuccessful: true } memberParseResult)
            {
                await using (context.BeginYield())
                {
                    member = await context.GetMemberAsync(memberParseResult.Value);
                }
            }

            return member is { }
                ? Success(member)
                : Failure("Could not find member.");
        }
    }
}