using System;
using System.Threading.Tasks;

using Disqord;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.Checks.Parameters
{
    public sealed class MustBeOwnedAttribute : SbuParameterCheckAttribute
    {
        public bool MustBeOwned { get; }

        public MustBeOwnedAttribute(bool mustBeOwned = true) => MustBeOwned = mustBeOwned;

        protected override ValueTask<CheckResult> CheckAsync(object argument, SbuCommandContext context)
            => ((argument as ISbuOwnedEntity)!.OwnerId is { }) == MustBeOwned
                ? ParameterCheckAttribute.Success()
                : ParameterCheckAttribute.Failure(
                    MustBeOwned
                        ? string.Format(
                            "The {0} must not be owned.",
                            argument switch
                            {
                                SbuColorRole => "role",
                                SbuTag => "tag",
                                SbuReminder => "reminder",
                                _ => "entity"
                            }
                        )
                        : string.Format(
                            "The {0} must be owned, it is currently owned by {1}.",
                            argument switch
                            {
                                SbuColorRole => "role",
                                SbuTag => "tag",
                                SbuReminder => "reminder",
                                _ => "entity",
                            },
                            Mention.User((argument as ISbuOwnedEntity)!.OwnerId!.Value)
                        )
                );

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(ISbuOwnedEntity));
    }
}