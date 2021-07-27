using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    [Group("role", "r")]
    [RequireBotGuildPermissions(Permission.ManageRoles)]
    [Description("A collection of commands for creation, modification, removal and usage of color roles.")]
    [Remarks(
        "A user may only have one color role at a time, role colors can be given as hex codes starting with `#` or as "
        + "color name literals."
    )]
    public sealed partial class ColorRoleModule : SbuModuleBase
    {
        [Command]
        [Description("Displays information about your current color role.")]
        public DiscordCommandResult Role() => Context.Author.GetColorRole() is { } role
            ? Reply(
                new LocalEmbed()
                    .WithColor(role.Color)
                    .AddField("Name", role.Name)
                    .AddField("Color", role.Color)
            )
            : Reply("You have no color role.");
    }
}