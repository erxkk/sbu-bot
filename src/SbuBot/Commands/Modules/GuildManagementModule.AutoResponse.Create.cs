using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Kkommon;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Extensions;
using SbuBot.Models;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        public sealed partial class AutoResponseSubModule
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

                        default:
                            throw new ArgumentOutOfRangeException();
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

                        default:
                            throw new ArgumentOutOfRangeException();
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

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;

                        case Result<string, FollowUpError>.Error error:
                            return Reply(
                                error.Value == FollowUpError.Aborted
                                    ? "Aborted."
                                    : "Aborted: You did not provide an auto response."
                            );

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    ChatService service = Context.Services.GetRequiredService<ChatService>();

                    await service.SetAutoResponseAsync(
                        Context.Guild.Id,
                        trigger,
                        response
                    );

                    var context = Context.GetSbuDbContext();

                    await context.SaveChangesAsync();

                    return Reply("Auto response created.");
                }
            }
        }
    }
}