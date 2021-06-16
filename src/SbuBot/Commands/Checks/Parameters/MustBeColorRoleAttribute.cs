using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Checks.Parameters
{
    public sealed class MustBeColorRoleAttribute : DiscordGuildParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
            => SbuUtility.IsSbuColorRole((argument as IRole)!, (context.Bot as SbuBot)!)
                ? ParameterCheckAttribute.Success()
                : ParameterCheckAttribute.Failure("The given role must be a color role.");

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(IRole));
    }
}