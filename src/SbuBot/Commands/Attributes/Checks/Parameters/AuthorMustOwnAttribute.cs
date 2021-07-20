using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Kkommon.Exceptions;

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

            if (argument is IOneOrAll oneOrAll)
            {
                switch (oneOrAll)
                {
                    case IOneOrAll.IAll:
                        return Success();

                    case IOneOrAll.ISpecific specific:
                        bool authorDoesOwn = (specific.Value as ISbuOwnedEntity)!.OwnerId
                            == (await context.GetAuthorAsync()).Id;

                        return authorDoesOwn == AuthorMustOwn
                            ? Success()
                            : Failure(
                                string.Format(
                                    "You must {0}to be the owner of the given {1}.",
                                    AuthorMustOwn ? "" : "not ",
                                    specific.Value switch
                                    {
                                        SbuColorRole => "role",
                                        SbuTag => "tag",
                                        SbuReminder => "reminder",
                                        _ => "entity",
                                    }
                                )
                            );

                    default:
                        throw new UnreachableException();
                }
            }

            throw new UnreachableException("The given argument was neither IOneOrAll or ISbuOwnedEntity.", argument);
        }

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(ISbuOwnedEntity))
            || (type.GetGenericTypeDefinition() == typeof(OneOrAll<>)
                && type.GetGenericArguments()[0].IsAssignableTo(typeof(ISbuOwnedEntity)));
    }
}