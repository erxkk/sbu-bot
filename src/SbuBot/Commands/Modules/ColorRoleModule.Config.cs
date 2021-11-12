using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Attributes.Checks.Parameters;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Group("config")]
        [Description("A group of commands for configuring color role separators.")]
        public sealed class ConfigSubModule : SbuModuleBase
        {
            [Command("set")]
            public async Task<DiscordCommandResult> AddAsync(
                ColorRoleSeparatorType type,
                [Description("The role to use as a color separator.")]
                [RequireHierarchy(HierarchyComparison.Less, HierarchyComparisonContext.Bot)]
                IRole role
            )
            {
                SbuDbContext context = Context.GetSbuDbContext();
                SbuGuild? guild = await context.GetGuildAsync(Context.Guild);

                switch (type)
                {
                    case ColorRoleSeparatorType.Top:
                        guild!.ColorRoleTopId = role.Id;
                        break;

                    case ColorRoleSeparatorType.Bottom:
                        guild!.ColorRoleBottomId = role.Id;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await context.SaveChangesAsync();

                return Response(
                    string.Format(
                        "Set role as {0} separator.",
                        type switch
                        {
                            ColorRoleSeparatorType.Top => "top",
                            ColorRoleSeparatorType.Bottom => "bottom",
                            _ => throw new ArgumentOutOfRangeException(),
                        }
                    )
                );
            }

            [Command("unset")]
            public async Task<DiscordCommandResult> RemoveAsync(
                [Description("The role to use as a color separator.")]
                ColorRoleSeparatorType type
            )
            {
                SbuDbContext context = Context.GetSbuDbContext();
                SbuGuild? guild = await context.GetGuildAsync(Context.Guild);

                switch (type)
                {
                    case ColorRoleSeparatorType.Top:
                        guild!.ColorRoleTopId = null;
                        break;

                    case ColorRoleSeparatorType.Bottom:
                        guild!.ColorRoleBottomId = null;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await context.SaveChangesAsync();

                return Response(
                    string.Format(
                        "Unset {0} separator.",
                        type switch
                        {
                            ColorRoleSeparatorType.Top => "top",
                            ColorRoleSeparatorType.Bottom => "bottom",
                            _ => throw new ArgumentOutOfRangeException(),
                        }
                    )
                );
            }
        }
    }

    public enum ColorRoleSeparatorType : byte
    {
        Top,
        Bottom,
    }
}
