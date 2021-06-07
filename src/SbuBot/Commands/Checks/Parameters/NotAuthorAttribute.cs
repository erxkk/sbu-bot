using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.Checks.Parameters
{
    public sealed class NotAuthorAttribute : DiscordParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, DiscordCommandContext context)
            => argument switch
                {
                    IMember member => member.Id,
                    Snowflake snowflake => snowflake,
                    SbuMember sbuMember => sbuMember.DiscordId,
                    _ => throw new ArgumentOutOfRangeException(nameof(argument), argument, null),
                }
                != context.Author.Id
                    ? ParameterCheckAttribute.Success()
                    : ParameterCheckAttribute.Failure("This parameter cannot be the same as the command author.");

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(IMember))
            || type == typeof(Snowflake)
            || type == typeof(Snowflake?)
            || type == typeof(SbuMember);
    }
}