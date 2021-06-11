using System;
using System.Threading.Tasks;

using Disqord;

using Qmmands;

namespace SbuBot.Commands.Checks.Parameters
{
    public sealed class MustBeColorRoleAttribute : SbuParameterCheckAttribute
    {
        protected override ValueTask<CheckResult> CheckAsync(object argument, SbuCommandContext context)
            => SbuUtility.IsSbuColorRole((argument as IRole)!)
                ? ParameterCheckAttribute.Success()
                : ParameterCheckAttribute.Failure("The given role must be a color role.");

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(IRole));
    }
}