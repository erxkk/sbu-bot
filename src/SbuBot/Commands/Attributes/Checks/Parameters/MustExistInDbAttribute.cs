using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public sealed class MustExistInDbAttribute : DiscordGuildParameterCheckAttribute
    {
        public bool MustExistInDb { get; }

        public MustExistInDbAttribute(bool mustExistInDb = true) => MustExistInDb = mustExistInDb;

        public override async ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
            => argument switch
            {
                IMember member => (await context.GetSbuDbContext()
                        .Members.FirstOrDefaultAsync(m => m.DiscordId == member.Id) is { })
                    == MustExistInDb
                        ? ParameterCheckAttribute.Success()
                        : ParameterCheckAttribute.Failure(
                            $"The given member must {(MustExistInDb ? "" : "not ")}be in the database for "
                            + "this command."
                        ),
                IRole role => (await context.GetSbuDbContext()
                        .ColorRoles.FirstOrDefaultAsync(m => m.DiscordId == role.Id) is { })
                    == MustExistInDb
                        ? ParameterCheckAttribute.Success()
                        : ParameterCheckAttribute.Failure(
                            $"The given role must {(MustExistInDb ? "" : "not ")}be in the database for "
                            + "this command."
                        ),
                _ => throw new ArgumentException($"Invalid argument type: {argument.GetType()}", nameof(argument)),
            };

        public override bool CheckType(Type type)
            => type.IsAssignableTo(typeof(IMember)) || type.IsAssignableTo(typeof(IRole));
    }
}