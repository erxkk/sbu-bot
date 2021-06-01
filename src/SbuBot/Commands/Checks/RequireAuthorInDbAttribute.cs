using System.Threading.Tasks;

using Qmmands;

namespace SbuBot.Commands.Checks
{
    public sealed class RequireAuthorInDbAttribute : SbuCheckAttribute
    {
        public bool RequireAuthorInDb { get; }

        public RequireAuthorInDbAttribute(bool requireAuthorInDb = true) => RequireAuthorInDb = requireAuthorInDb;

        protected override ValueTask<CheckResult> CheckAsync(SbuCommandContext context)
            => context.Invoker is { } == RequireAuthorInDb
                ? CheckAttribute.Success()
                : CheckAttribute.Failure(
                    $"You must {(RequireAuthorInDb ? "" : "not ")}be in the database to use this command."
                );
    }
}