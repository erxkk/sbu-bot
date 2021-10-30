using Disqord;
using Disqord.Bot;

using Qmmands;

using SbuBot.Extensions;
using SbuBot.Services;

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
        private const string ROLE_DOES_NOT_EXIST
            = "The role doesn't exist, was it deleted? Make sure to remove it before creating a new role with `sbu role"
            + " delete`";

        private const string ROLE_HAS_HIGHER_HIERARCHY_FORMAT
            = "I cant {0} the role, its above mine.";

        public ConsistencyService ConsistencyService { get; set; } = null!;

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