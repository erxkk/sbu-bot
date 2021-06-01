using System;
using System.Threading.Tasks;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.Checks.Parameters
{
    public sealed class AuthorMustOwnAttribute : SbuParameterCheckAttribute
    {
        public bool AuthorMustOwn { get; }

        public AuthorMustOwnAttribute(bool authorMustOwn = true) => AuthorMustOwn = authorMustOwn;

        protected override ValueTask<CheckResult> CheckAsync(object argument, SbuCommandContext context)
            => ((argument as ISbuOwnedEntity)!.OwnerId == context.Author.Id) == AuthorMustOwn
                ? ParameterCheckAttribute.Success()
                : ParameterCheckAttribute.Failure(
                    string.Format(
                        "You must {0}to be the owner of the given {1}.",
                        AuthorMustOwn ? "" : "not ",
                        argument switch
                        {
                            SbuColorRole => "role",
                            SbuTag => "tag",
                            SbuReminder => "reminder",
                            _ => "entity",
                        }
                    )
                );

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(ISbuOwnedEntity));
    }
}