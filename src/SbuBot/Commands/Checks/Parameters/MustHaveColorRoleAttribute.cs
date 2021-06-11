using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Gateway;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.Checks.Parameters
{
    public sealed class MustHaveColorRoleAttribute : SbuParameterCheckAttribute
    {
        public bool MustHaveColorRole { get; }

        public MustHaveColorRoleAttribute(bool mustHaveColorRole = true) => MustHaveColorRole = mustHaveColorRole;

        protected override ValueTask<CheckResult> CheckAsync(object argument, SbuCommandContext context)
        {
            IMember member;

            switch (argument)
            {
                case IMember imember:
                    member = imember;
                    break;

                case SbuMember sbuMember:
                    if (context.Guild.GetMember(sbuMember.DiscordId) is not { } cachedMember)
                        throw new RequiredCacheException("Could not get required cached required.");

                    member = cachedMember;
                    break;

                default:
                    throw new ArgumentNullException(nameof(argument));
            }

            return SbuUtility.GetSbuColorRole(member) is { } == MustHaveColorRole
                ? ParameterCheckAttribute.Success()
                : ParameterCheckAttribute.Failure(
                    string.Format("The given member must {0}have a color role.", MustHaveColorRole ? "" : "no ")
                );
        }

        public override bool CheckType(Type type) => type.IsAssignableTo(typeof(IMember)) || type == typeof(SbuMember);
    }
}