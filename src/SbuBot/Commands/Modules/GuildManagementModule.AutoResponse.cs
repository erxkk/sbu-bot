using System.Collections.Generic;
using System.Linq;

using Disqord;
using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes.Checks;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Group("auto")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        [RequireGuildConfig(SbuGuildConfig.Respond)]
        [Description("A group of commands for creating and removing auto responses.")]
        public sealed partial class AutoResponseSubModule : SbuModuleBase
        {
            [Command("list")]
            [Description("Lists the auto responses of this server.")]
            public DiscordCommandResult List()
            {
                ChatService service = Context.Services.GetRequiredService<ChatService>();

                IReadOnlyDictionary<string, string> autoResponses = service.GetAutoResponses(Context.GuildId);

                if (autoResponses.Count == 0)
                    return Reply("This server has not auto responses.");

                return DistributedPages(
                    autoResponses.Select(ar => $"{SbuGlobals.BULLET} {ar.Key}\n`{ar.Value}`\n"),
                    embedFactory: embed => embed.WithTitle("Auto Responses")
                );
            }
        }
    }
}
