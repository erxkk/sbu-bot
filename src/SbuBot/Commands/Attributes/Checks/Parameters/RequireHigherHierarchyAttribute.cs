using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Kkommon.Exceptions;

using Qmmands;

using SbuBot.Exceptions;
using SbuBot.Extensions;
using SbuBot.Models;

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
                SbuColorRole sbuRole => (
                    context.Guild.Roles.TryGetValue(sbuRole.Id, out var discordRole)
                        ? discordRole.Position
                        : throw new NotCachedException("Could not find role in cache for hierarchy check."), "role"),
                _ => throw new UnreachableException($"Invalid argument type: {argument.GetType()}", argument),
            };

            return context.Author.GetHierarchy() > targetHierarchy
                ? Success()
                : Failure(
                    $"The hierarchy of the {type} must be lower than yours."
                );
        }

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(IMember))
            || type.IsAssignableTo(typeof(IRole))
            || type == typeof(SbuColorRole);
    }
}