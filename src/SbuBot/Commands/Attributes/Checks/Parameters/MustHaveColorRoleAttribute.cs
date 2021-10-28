using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon.Exceptions;

using Qmmands;

using SbuBot.Exceptions;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public sealed class MustHaveColorRoleAttribute : DiscordGuildParameterCheckAttribute
    {
        public bool MustHaveColorRole { get; }

        public MustHaveColorRoleAttribute(bool mustHaveColorRole = true) => MustHaveColorRole = mustHaveColorRole;

        public override async ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            IMember member;

            switch (argument)
            {
                case IMember iMember:
                    member = iMember;
                    break;

                case SbuMember sbuMember:
                    member = context.Guild.GetMember(sbuMember.Id) is { } cachedMember
                        ? cachedMember
                        : await context.Guild.FetchMemberAsync(sbuMember.Id);

                    break;

                default:
                    throw new UnreachableException($"Invalid argument type: {argument.GetType()}", argument);
            }

            return member.GetColorRole() is { } == MustHaveColorRole
                ? Success()
                : Failure(
                    string.Format("The given member must {0}have a color role.", MustHaveColorRole ? "" : "not ")
                );
        }

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(IMember)) || type == typeof(SbuMember);
    }
}