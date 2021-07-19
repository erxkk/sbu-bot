using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

using Kkommon;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Parsing;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Group("auto")]
        [RequireAdmin, RequireGuild(SbuGlobals.Guild.Sbu.SELF)]
        [Description("A group of commands for creating and removing auto responses.")]
        public sealed class AutoResponseSubModule : SbuModuleBase
        {
            [Group("create")]
            [Description("Creates a new auto response.")]
            public sealed class CreateGroup : SbuModuleBase
            {
                [Command]
                [Usage(
                    "auto create refuses to elaborate :: https://cdn.discordapp.com/attachments/820403561526722570/"
                    + "848874138587758592/Screenshot_20210530_010226.png",
                    "auto make what da dog doin :: what is the canine partaking in",
                    "auto mk h :: h"
                )]
                public async Task<DiscordCommandResult> CreateAsync(
                    [Description("The auto response descriptor.")]
                    AutoResponseDescriptor descriptor
                )
                {
                    ChatService service = Context.Services.GetRequiredService<ChatService>();

                    if (service.GetAutoResponse(Context.GuildId, descriptor.Trigger) is { })
                        return Reply("An auto response with same name already exists.");

                    await service.SetAutoResponseAsync(
                        Context.Guild.Id,
                        descriptor.Trigger,
                        descriptor.Response
                    );

                    return Reply("Auto response created.");
                }

                [Command]
                public async Task<DiscordCommandResult> CreateInteractiveAsync()
                {
                    string? trigger;

                    switch (await Context.WaitFollowUpForAsync(
                        "What should trigger the auto response? (spaces are allowed)."
                    ))
                    {
                        case Result<string, FollowUpError>.Success followUp:
                            trigger = followUp.Value.Trim();
                            break;

                        case Result<string, FollowUpError>.Error error:
                            return Reply(
                                error.Value == FollowUpError.Aborted
                                    ? "Aborted."
                                    : "Aborted: You did not provide an auto response trigger."
                            );

                        // unreachable
                        default:
                            throw new();
                    }

                    switch (SbuAutoResponse.IsValidTrigger(trigger))
                    {
                        case SbuAutoResponse.ValidTriggerType.TooLong:
                        {
                            return Reply(
                                string.Format(
                                    "Aborted: The auto response trigger can be at most {0} characters long.",
                                    SbuTag.MAX_NAME_LENGTH
                                )
                            );
                        }

                        case SbuAutoResponse.ValidTriggerType.Reserved:
                            return Reply("The auto response trigger cannot be a reserved keyword.");

                        case SbuAutoResponse.ValidTriggerType.Valid:
                            break;

                        // unreachable
                        default:
                            throw new();
                    }

                    if (await Context.GetAutoResponseAsync(trigger) is { })
                        return Reply("Auto response with same name already exists.");

                    string? response;

                    switch (await Context.WaitFollowUpForAsync("What do you want the bot to respond with?"))
                    {
                        case Result<string, FollowUpError>.Success followUp:
                            response = followUp.Value.Trim();

                            switch (SbuAutoResponse.IsValidResponse(response))
                            {
                                case SbuAutoResponse.ValidResponseType.TooLong:
                                {
                                    return Reply(
                                        string.Format(
                                            "Aborted: The auto response can be at most {0} characters long.",
                                            SbuAutoResponse.MAX_LENGTH
                                        )
                                    );
                                }

                                case SbuAutoResponse.ValidResponseType.Valid:
                                    break;

                                // unreachable
                                default:
                                    throw new();
                            }

                            break;

                        case Result<string, FollowUpError>.Error error:
                            return Reply(
                                error.Value == FollowUpError.Aborted
                                    ? "Aborted."
                                    : "Aborted: You did not provide an auto response."
                            );

                        // unreachable
                        default:
                            throw new();
                    }

                    ChatService service = Context.Services.GetRequiredService<ChatService>();

                    await service.SetAutoResponseAsync(
                        Context.Guild.Id,
                        trigger,
                        response
                    );

                    await Context.SaveChangesAsync();

                    return Reply("Auto response created.");
                }
            }

            [Command("list")]
            [Description("Lists the auto responses of this server.")]
            public DiscordCommandResult List()
            {
                ChatService service = Context.Services.GetRequiredService<ChatService>();

                IReadOnlyDictionary<string, string> autoResponses = service.GetAutoResponses(Context.GuildId);

                if (autoResponses.Count == 0)
                    return Reply("This server has not auto responses.");

                return DistributedPages(
                    autoResponses.Select(ar => $"**Trigger:** {ar.Key}\n**Response:** {ar.Value}"),
                    embedFactory: embed => embed.WithTitle("Auto Responses")
                );
            }

            [Command("delete")]
            [Description("Removes a given auto response.")]
            [Usage("auto remove what da dog doin", "auto delete h", "auto rm all")]
            public async Task<DiscordCommandResult> RemoveAsync(
                [Description("The auto response that should be removed.")]
                OneOrAll<SbuAutoResponse> autoResponse
            )
            {
                ChatService service = Context.Services.GetRequiredService<ChatService>();

                switch (autoResponse)
                {
                    case OneOrAll<SbuAutoResponse>.All:
                    {
                        ConfirmationResult result = await Context.WaitForConfirmationAsync(
                            "Are you sure you want to remove all auto responses? Respond `yes` to confirm."
                        );

                        switch (result)
                        {
                            case ConfirmationResult.Timeout:
                            case ConfirmationResult.Aborted:
                                return Reply("Aborted.");

                            case ConfirmationResult.Confirmed:
                                break;

                            // unreachable
                            default:
                                throw new();
                        }

                        await service.RemoveAutoResponsesAsync(Context.GuildId);

                        return Reply("All auto responses removed.");
                    }

                    case OneOrAll<SbuAutoResponse>.Specific specific:
                    {
                        ConfirmationResult result = await Context.WaitForConfirmationAsync(
                            "Are you sure you want to remove this tag? Respond `yes` to confirm."
                        );

                        switch (result)
                        {
                            case ConfirmationResult.Timeout:
                            case ConfirmationResult.Aborted:
                                return Reply("Aborted.");

                            case ConfirmationResult.Confirmed:
                                break;

                            // unreachable
                            default:
                                throw new();
                        }

                        await service.RemoveAutoResponseAsync(Context.GuildId, specific.Value.Trigger);

                        return Reply("Auto response removed.");
                    }

                    // unreachable
                    default:
                        throw new();
                }
            }
        }
    }
}