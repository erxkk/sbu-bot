using System;
using System.Reflection;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Parsing;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public sealed class AuthorMustOwnAttribute : DiscordGuildParameterCheckAttribute
    {
        public bool AuthorMustOwn { get; }

        public AuthorMustOwnAttribute(bool authorMustOwn = true) => AuthorMustOwn = authorMustOwn;

        public override async ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            if (argument is ISbuOwnedEntity entity)
            {
                bool authorDoesOwn = entity.OwnerId == (await context.GetAuthorAsync()).Id;

                return authorDoesOwn == AuthorMustOwn
                    ? Success()
                    : Failure(
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
            }

            // this check makes no sense, when all are specified it's queried in the command anyway
            if (argument.GetType().GetGenericTypeDefinition() == typeof(OneOrAll<>))
                return Success();

            // unreachable
            throw new();
        }

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(ISbuOwnedEntity))
            || (type.GetGenericTypeDefinition() == typeof(OneOrAll<>)
                && type.GetGenericArguments()[0].IsAssignableTo(typeof(ISbuOwnedEntity)));
    }
}