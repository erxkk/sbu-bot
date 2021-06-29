using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public sealed class AuthorMustOwnAttribute : DiscordGuildParameterCheckAttribute
    {
        public bool AuthorMustOwn { get; }

        public AuthorMustOwnAttribute(bool authorMustOwn = true) => AuthorMustOwn = authorMustOwn;

        public override async ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
            => ((argument as ISbuOwnedEntity)!.OwnerId
                    == (await context.GetSbuDbContext().GetMemberAsync(context.Author))!.Id)
                == AuthorMustOwn
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