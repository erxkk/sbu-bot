using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Information;

namespace SbuBot.Commands.Modules
{
    public sealed class GuildManagementModule : SbuModuleBase
    {
        // TODO: TEST
        [Group("pin"), RequireAuthorRole(SbuBotGlobals.Roles.PIN_BRIGADE)]
        public sealed class PinGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> PinMessageAsync(
                [OverrideDefault("@reply")] IUserMessage? message = null,
                bool force = false
            )
            {
                if (message is null)
                {
                    if (!Context.Message.ReferencedMessage.HasValue)
                        return Reply("You need to provide a message or reply to one.");

                    message = Context.Message.ReferencedMessage.Value;
                }

                if (Context.Bot.GetChannel(SbuBotGlobals.Guild.ID, SbuBotGlobals.Channels.PIN_ARCHIVE)
                    is not ITextChannel pinArchive)
                    throw new RequiredCacheException("Could not find required pin archive channel.");

                if (message is not IGatewayUserMessage gatewayUserMessage)
                    throw new RequiredCacheException("Message was not a gateway user message.");

                switch (Utility.TryCreatePinMessage(gatewayUserMessage, force))
                {
                    case Result<LocalMessage, string>.Success pinMessage:
                        await pinArchive.SendMessageAsync(pinMessage);

                        if (gatewayUserMessage.IsPinned)
                            await gatewayUserMessage.UnpinAsync();

                        return null!;

                    case Result<LocalMessage, string>.Error error:
                        return Reply(error);

                    default:
                        return null!;
                }
            }

            [Command("all")]
            public async Task<DiscordCommandResult> PinAllAsync(
                [OverrideDefault("#here")] ITextChannel? channel = null,
                bool force = false
            )
            {
                channel ??= Context.Channel;

                if (Context.Bot.GetChannel(SbuBotGlobals.Guild.ID, SbuBotGlobals.Channels.PIN_ARCHIVE)
                    is not ITextChannel pinArchive)
                    throw new RequiredCacheException("Could not find required pin archive channel.");

                foreach (IUserMessage message in await channel.FetchPinnedMessagesAsync())
                {
                    switch (Utility.TryCreatePinMessage(message, force))
                    {
                        case Result<LocalMessage, string>.Success pinMessage:
                            await pinArchive.SendMessageAsync(pinMessage);
                            await message.UnpinAsync();

                            break;

                        case Result<LocalMessage, string>.Error error:
                            await Reply(error);
                            break;

                        default:
                            continue;
                    }
                }

                return Reply("Finished.");
            }
        }
    }
}