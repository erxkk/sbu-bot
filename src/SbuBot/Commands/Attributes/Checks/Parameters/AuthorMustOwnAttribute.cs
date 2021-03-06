using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Parsing.HelperTypes;
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
                bool authorDoesOwn = entity.OwnerId == (await context.GetDbAuthorAsync()).Id;

                return authorDoesOwn == AuthorMustOwn
                    ? Success()
                    : Failure(
                        string.Format(
                            "You must {0}be the owner of the given {1}.",
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
                if (oneOrAll.IsAll)
                    return Success();

                bool authorDoesOwn = (oneOrAll.Value as ISbuOwnedEntity)!.OwnerId
                    == (await context.GetDbAuthorAsync()).Id;

                return authorDoesOwn == AuthorMustOwn
                    ? Success()
                    : Failure(
                        string.Format(
                            "You must {0}be the owner of the given {1}.",
                            AuthorMustOwn ? "" : "not ",
                            oneOrAll.Value switch
                            {
                                SbuColorRole => "role",
                                SbuTag => "tag",
                                SbuReminder => "reminder",
                                _ => "entity",
                            }
                        )
                    );
            }

            throw new ArgumentOutOfRangeException();
        }

        public override bool CheckType(Type type)
            => type.IsAssignableTo(typeof(ISbuOwnedEntity))
                || (type.GetGenericTypeDefinition() == typeof(OneOrAll<>)
                    && type.GetGenericArguments()[0].IsAssignableTo(typeof(ISbuOwnedEntity)));
    }
}
