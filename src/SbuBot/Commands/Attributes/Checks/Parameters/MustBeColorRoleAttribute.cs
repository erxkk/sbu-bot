using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public sealed class MustBeColorRoleAttribute : DiscordGuildParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
            => (argument as IRole)!.Color is { }
                ? Success()
                : Failure("The given role must be a color role.");

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(IRole));
    }
}