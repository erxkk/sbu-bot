using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

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
        public override async ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            int targetHierarchy;
            string type;

            switch (argument)
            {
                case IMember member:
                    targetHierarchy = member.GetHierarchy();
                    type = "member";
                    break;

                case IRole role:
                    targetHierarchy = role.Position;
                    type = "role";
                    break;

                case SbuColorRole sbuRole:
                {
                    IRole? role = context.Guild.Roles.GetValueOrDefault(sbuRole.Id);

                    if (role is null)
                    {
                        IReadOnlyList<IRole> roles = await context.Bot.FetchRolesAsync(context.GuildId);
                        role = roles.FirstOrDefault(r => r.Id == sbuRole.Id);
                    }

                    if (role is null)
                        throw new NotCachedException("Could not find role in cache for hierarchy check.");

                    targetHierarchy = role.Position;
                    type = "role";
                    break;
                }

                default:
                    throw new UnreachableException($"Invalid argument type: {argument.GetType()}", argument);
            }

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