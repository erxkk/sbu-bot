using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Microsoft.EntityFrameworkCore;

using Qmmands;

using SbuBot.Commands.Parsing.HelperTypes;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    [Group("colorrole", "crole", "cr")]
    [RequireBotGuildPermissions(Permission.ManageRoles)]
    [Description("A collection of commands for creation, modification, removal and usage of color roles.")]
    [Remarks(
        "A user may only have one color role at a time, role colors can be given as hex codes starting with `#` or as "
        + "color name literals."
    )]
    public sealed partial class ColorRoleModule : SbuModuleBase
    {
        public ConsistencyService ConsistencyService { get; set; } = null!;

        [Command]
        [Description("Displays information about your current color role, shortcut for `sbu colorrole list`.")]
        public DiscordCommandResult Role() => Context.Author.GetColorRole() is { } role
            ? Response(
                new LocalEmbed()
                    .WithAuthor(Context.Author)
                    .WithColor(role.Color)
                    .AddField("Name", role.Name)
                    .AddField("Color", role.Color)
            )
            : Reply("You have no color role.");

        [Command("list")]
        [Description("Lists the tags of a given member, or of the command author if no member is specified.")]
        public async Task<DiscordCommandResult> ListAsync(OneOrAll<SbuMember>? owner = null)
        {
            if (owner is null || !owner.IsAll)
            {
                SbuMember ownerOrAuthor = owner?.Value ?? await Context.GetDbAuthorAsync();

                if (ownerOrAuthor.ColorRole is null)
                    return Reply($"{Mention.User(ownerOrAuthor.Id)} has no color role");

                IMember author = Context.Guild.GetMember(ownerOrAuthor.Id)
                    ?? await Context.Guild.FetchMemberAsync(ownerOrAuthor.Id);

                IRole? role = Context.Guild.Roles.GetValueOrDefault(ownerOrAuthor.Id);

                if (role is null)
                    return Reply("The color role could not be found.");

                return Response(
                    new LocalEmbed()
                        .WithAuthor(author)
                        .WithColor(role.Color)
                        .AddField("Name", role.Name)
                        .AddField("Color", role.Color)
                );
            }
            else
            {
                SbuDbContext context = Context.GetSbuDbContext();

                List<SbuColorRole> colorRoles = await context.ColorRoles
                    .Where(r => r.GuildId == Context.GuildId)
                    .ToListAsync(Bot.StoppingToken);

                return DistributedPages(
                    colorRoles.Select(
                        r => string.Format(
                            "{0} `{1}`\n{2}\n",
                            SbuGlobals.BULLET,
                            Mention.Role(r.Id),
                            r.OwnerId is null ? "`No Owner`" : Mention.User(r.OwnerId.Value)
                        )
                    ),
                    embedFactory: embed => embed.WithTitle("Color Roles"),
                    itemsPerPage: 20,
                    maxPageLength: LocalEmbed.MaxDescriptionLength / 2
                );
            }
        }
    }
}
