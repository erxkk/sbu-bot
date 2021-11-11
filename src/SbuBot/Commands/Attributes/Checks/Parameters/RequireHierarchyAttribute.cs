using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Models;

namespace SbuBot.Commands.Attributes.Checks.Parameters
{
    public class RequireHierarchyAttribute : DiscordGuildParameterCheckAttribute
    {
        public int Value { get; }
        public HierarchyComparisonContext ComparisonContext { get; }
        public HierarchyComparison Comparison { get; }

        public RequireHierarchyAttribute(HierarchyComparison comparison, int value)
        {
            Value = value;
            Comparison = comparison;
        }

        public RequireHierarchyAttribute(HierarchyComparison comparison, HierarchyComparisonContext comparisonContext)
        {
            if (comparisonContext == HierarchyComparisonContext.Literal)
                throw new ArgumentException("Literal is implicitly set giving the attribute an integer value");

            ComparisonContext = comparisonContext;
            Comparison = comparison;
        }

        public override async ValueTask<CheckResult> CheckAsync(object argument, DiscordGuildCommandContext context)
        {
            string argumentType;
            int argumentHierarchy;

            switch (argument)
            {
                case IRole iRole:
                {
                    argumentType = "role";
                    argumentHierarchy = iRole.Position;
                    break;
                }

                case IMember iMember:
                {
                    argumentType = "member";
                    argumentHierarchy = iMember.GetHierarchy();
                    break;
                }

                case SbuColorRole sbuRole:
                {
                    if (context.Guild.Roles.GetValueOrDefault(sbuRole.Id) is { } role)
                        argumentHierarchy = role.Position;
                    else
                        return Failure(SbuUtility.Format.DoesNotExist("The role"));

                    argumentType = "role";
                    break;
                }

                case SbuRole sbuRole:
                {
                    if (context.Guild.Roles.GetValueOrDefault(sbuRole.Id) is { } role)
                        argumentHierarchy = role.Position;
                    else
                        return Failure(SbuUtility.Format.DoesNotExist("The role"));

                    argumentType = "member";
                    break;
                }

                case SbuMember sbuMember:
                {
                    IMember member = context.Guild.Members.GetValueOrDefault(sbuMember.Id)
                        ?? await context.Guild.FetchMemberAsync(sbuMember.Id);

                    if (member is null)
                        return Failure(SbuUtility.Format.DoesNotExist("The member"));

                    argumentType = "member";
                    argumentHierarchy = member.GetHierarchy();
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            (string? targetType, int targetHierarchy) = ComparisonContext switch
            {
                HierarchyComparisonContext.Bot => ("bot", context.CurrentMember.GetHierarchy()),
                HierarchyComparisonContext.Author => ("author", context.Author.GetHierarchy()),
                HierarchyComparisonContext.Literal => (null, Value),
                _ => throw new ArgumentOutOfRangeException(),
            };

            string? failureReason = Comparison switch
            {
                HierarchyComparison.Greater
                    => argumentHierarchy > targetHierarchy ? null : "greater than",
                HierarchyComparison.GreaterThanOrEqual
                    => argumentHierarchy >= targetHierarchy ? null : "greater than or equal to",
                HierarchyComparison.Less
                    => argumentHierarchy < targetHierarchy ? null : "less than",
                HierarchyComparison.LessThanOrEqual
                    => argumentHierarchy <= targetHierarchy ? null : "less than or equal to",
                HierarchyComparison.Equal
                    => argumentHierarchy == targetHierarchy ? null : "equal to",
                HierarchyComparison.Unequal
                    => argumentHierarchy != targetHierarchy ? null : "unequal to",
                _ => throw new ArgumentOutOfRangeException(),
            };

            return failureReason is null
                ? ParameterCheckAttribute.Success()
                : ParameterCheckAttribute.Failure(
                    string.Format(
                        "The given {0}'s hierarchy ({1}) must be {2} {3}.",
                        argumentType,
                        argumentHierarchy,
                        failureReason,
                        targetType == null ? targetHierarchy : $"the {targetType}'s hierarchy ({targetHierarchy})"
                    )
                );
        }

        public override bool CheckType(Type type)
            => type.IsAssignableTo(typeof(IRole))
                || type.IsAssignableTo(typeof(IMember))
                || type == typeof(SbuRole)
                || type == typeof(SbuColorRole)
                || type == typeof(SbuMember);
    }
}

namespace SbuBot.Commands.Attributes.Checks
{
    public enum HierarchyComparisonContext : byte
    {
        Bot,
        Author,
        Literal,
    }

    public enum HierarchyComparison : byte
    {
        Greater,
        GreaterThanOrEqual,
        Less,
        LessThanOrEqual,
        Equal,
        Unequal,
    }
}