using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Kkommon.Exceptions;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Parsing;
using SbuBot.Commands.Views;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        public sealed partial class AutoResponseSubModule
        {
            [Command("delete")]
            [Description("Removes a given auto response.")]
            [Usage("auto remove what da dog doin", "auto delete h", "auto rm all")]
            public async Task<DiscordCommandResult> DeleteAsync(
                [Description("The auto response that should be removed.")]
                OneOrAll<SbuAutoResponse> autoResponse
            )
            {
                ChatService service = Context.Services.GetRequiredService<ChatService>();

                switch (autoResponse)
                {
                    case OneOrAll<SbuAutoResponse>.All:
                    {
                        ConfirmationState result = await ConfirmationAsync(
                            "Auto Response Removal",
                            "Are you sure you want to remove all auto responses?"
                        );

                        switch (result)
                        {
                            case ConfirmationState.None:
                            case ConfirmationState.Aborted:
                                return Reply("Aborted.");

                            case ConfirmationState.Confirmed:
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        await service.RemoveAutoResponsesAsync(Context.GuildId);

                        return Reply("All auto responses removed.");
                    }

                    case OneOrAll<SbuAutoResponse>.Specific specific:
                    {
                        ConfirmationState result = await ConfirmationAsync(
                            "Auto Response Removal",
                            "Are you sure you want to remove this tag?"
                        );

                        switch (result)
                        {
                            case ConfirmationState.None:
                            case ConfirmationState.Aborted:
                                return Reply("Aborted.");

                            case ConfirmationState.Confirmed:
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        await service.RemoveAutoResponseAsync(Context.GuildId, specific.Value.Trigger);

                        return Reply("Auto response removed.");
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}