using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public sealed class MustExistInDbAttribute : DiscordGuildParameterCheckAttribute
    {
        public bool MustExistInDb { get; }

        public MustExistInDbAttribute(bool mustExistInDb = true) => MustExistInDb = mustExistInDb;

        public override async ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            (bool exists, string type) = argument switch
            {
                IMember member => (await context.GetDbMemberAsync(member) is { }, "member"),
                IRole role => (await context.GetDbColorRoleAsync(role) is { }, "role"),
                _ => throw new ArgumentOutOfRangeException(),
            };

            return exists == MustExistInDb
                ? Success()
                : Failure($"The given {type} must {(MustExistInDb ? "" : "not ")}be in the database for this command.");
        }

        public override bool CheckType(Type type)
            => type.IsAssignableTo(typeof(IMember))
                || type.IsAssignableTo(typeof(IRole));
    }
}