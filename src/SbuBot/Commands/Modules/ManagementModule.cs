using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Information;
using SbuBot.Services;

namespace SbuBot.Commands.Modules
{
    public sealed class ManagementModule : SbuModuleBase
    {
        [Group("pin"), RequireAuthorRole(SbuBotGlobals.Roles.PIN_BRIGADE)]
        public sealed class PinGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> PinMessageAsync(
                [OverrideDefault("reply")] IUserMessage? message = null,
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
                    throw new RequiredCacheException("Could not find pin archive.");

                if (message is not IGatewayUserMessage gatewayUserMessage)
                    throw new RequiredCacheException("Message was not a gateway user message.");

                ManagementService service = Context.Services.GetRequiredService<ManagementService>();

                switch (service.TryCreatePinMessage(gatewayUserMessage, force))
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

            [Command("all"), Disabled]
            public DiscordCommandResult PinAllAsync(ITextChannel? channel = null) => null!;
        }
    }
}