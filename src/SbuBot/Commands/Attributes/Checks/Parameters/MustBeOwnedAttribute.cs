using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public sealed class MustBeOwnedAttribute : DiscordGuildParameterCheckAttribute
    {
        public bool MustBeOwned { get; }

        public MustBeOwnedAttribute(bool mustBeOwned = true) => MustBeOwned = mustBeOwned;

        public override ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            ISbuOwnedEntity ownedEntity = (argument as ISbuOwnedEntity)!;

            return (ownedEntity.OwnerId is { }) == MustBeOwned
                ? Success()
                : Failure(
                    string.Format(
                        MustBeOwned
                            ? "The {0} must be owned, but is not."
                            : "The {0} must not be owned, but is currently owned by somebody else.",
                        argument switch
                        {
                            SbuColorRole => "role",
                            SbuTag => "tag",
                            SbuReminder => "reminder",
                            _ => "entity",
                        }
                    )
                );
        }

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(ISbuOwnedEntity));
    }
}
