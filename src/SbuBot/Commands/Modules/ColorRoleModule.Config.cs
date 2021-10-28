using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class ColorRoleModule
    {
        [Group("separator")]
        [Description("A group of commands for configuring color role separators.")]
        public sealed class ConfigSubModule : SbuModuleBase
        {
            [Command("set")]
            [Usage(
                "role separator set top some role name",
                "r separator set top 732234804384366602",
                "r separator set bottom @SBU-Bot"
            )]
            public async Task<DiscordCommandResult> AddAsync(
                ColorRoleSeparatorType type,
                [Description("The role to use as a color separator.")]
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
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                await context.SaveChangesAsync();

                return Reply(
                    string.Format(
                        "Set role as {0} separator.",
                        type switch
                        {
                            ColorRoleSeparatorType.Top => "top",
                            ColorRoleSeparatorType.Bottom => "bottom",
                        }
                    )
                );
            }

            [Command("unset")]
            [Usage("role separator unset top", "r separator unset top", "r separator unset bottom")]
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
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                await context.SaveChangesAsync();

                return Reply(
                    string.Format(
                        "Unset {0} separator.",
                        type switch
                        {
                            ColorRoleSeparatorType.Top => "top",
                            ColorRoleSeparatorType.Bottom => "bottom",
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