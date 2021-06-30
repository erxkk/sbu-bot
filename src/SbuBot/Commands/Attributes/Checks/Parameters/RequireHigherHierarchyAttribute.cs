using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RequireHigherHierarchyAttribute : DiscordGuildParameterCheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            (int targetHierarchy, string type) = argument switch
            {
                IMember member => (member.GetHierarchy(), "member"),
                IRole role => (role.Position, "role"),
                _ => throw new ArgumentOutOfRangeException(nameof(argument), argument, null),
            };

            return context.Author.GetHierarchy() > targetHierarchy
                ? ParameterCheckAttribute.Success()
                : ParameterCheckAttribute.Failure(
                    $"The hierarchy of the {type} must be lower than yours."
                );
        }

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(IMember))
            || type.IsAssignableTo(typeof(IRole));
    }
}