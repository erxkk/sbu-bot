using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Parsing.HelperTypes;
using SbuBot.Commands.Views;
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

            [Command("delete")]
            [Description("Removes a given auto response.")]
            [UsageOverride("auto remove what da dog doin", "auto delete h", "auto rm all")]
            public async Task<DiscordCommandResult> DeleteAsync(
                [Description("The auto response that should be removed.")]
                OneOrAll<SbuAutoResponse> autoResponse
            )
            {
                ChatService service = Context.Services.GetRequiredService<ChatService>();

                if (autoResponse.IsAll)
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Auto Response Removal",
                        "Are you sure you want to remove **all** auto responses?"
                    );

                    switch (result)
                    {
                        case ConfirmationState.None:
                        case ConfirmationState.Aborted:
                        case ConfirmationState.TimedOut:
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    await service.RemoveAutoResponsesAsync(Context.GuildId);

                    return Reply("All auto responses removed.");
                }
                else
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Auto Response Removal",
                        "Are you sure you want to remove this tag?"
                    );

                    switch (result)
                    {
                        case ConfirmationState.None:
                        case ConfirmationState.Aborted:
                        case ConfirmationState.TimedOut:
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    await service.RemoveAutoResponseAsync(Context.GuildId, autoResponse.Value.Trigger);

                    return Response("Auto response removed.");
                }
            }
        }
    }
}
