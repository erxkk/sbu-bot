using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

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
            switch (argument)
            {
                case IMember member:
                {
                    var exists = await context.GetDbMemberAsync(member) is { };

                    return exists == MustExistInDb
                        ? Success()
                        : Failure(
                            $"The given member must {(MustExistInDb ? "" : "not ")}be in the database for "
                            + "this command."
                        );
                }

                case IRole role:
                {
                    var exists = await context.GetDbColorRoleAsync(role) is { };

                    return exists == MustExistInDb
                        ? Success()
                        : Failure(
                            $"The given role must {(MustExistInDb ? "" : "not ")}be in the database for "
                            + "this command."
                        );
                }

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(argument),
                        argument,
                        $"Invalid argument type: {argument.GetType()}"
                    );
            }
        }

        public override bool CheckType(Type type)
            => type.IsAssignableTo(typeof(IMember)) || type.IsAssignableTo(typeof(IRole));
    }
}