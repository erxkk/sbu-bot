using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Kkommon.Exceptions;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Group("auto")]
        [RequireAdmin, RequireGuild(SbuGlobals.Guild.Sbu.SELF)]
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
                    autoResponses.Select(ar => $"{ar.Key}\n`{ar.Value}`\n"),
                    embedFactory: embed => embed.WithTitle("Auto Responses")
                );
            }
        }
    }
}