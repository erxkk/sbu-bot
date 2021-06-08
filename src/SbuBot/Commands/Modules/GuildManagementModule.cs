using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Information;

namespace SbuBot.Commands.Modules
{
    public sealed class GuildManagementModule : SbuModuleBase
    {
        [Group("archive"), RequireAuthorRole(SbuBotGlobals.Roles.PIN_BRIGADE, Group = "AdminOrPinBrigade"),
         RequireAuthorAdmin(Group = "AdminOrPinBrigade")]
        public sealed class PinGroup : SbuModuleBase
        {
            [Command]
            public async Task<DiscordCommandResult> ArchiveMessageAsync(
                [OverrideDefault("@reply")] IUserMessage? message = null,
                bool unpinOriginal = true
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

                switch (Utility.TryCreatePinMessage(message))
                {
                    case Result<LocalMessage, string>.Success pinMessage:
                        await pinArchive.SendMessageAsync(pinMessage);

                        if (unpinOriginal && message.IsPinned)
                            await message.UnpinAsync();

                        return null!;

                    case Result<LocalMessage, string>.Error error:
                        return Reply(error);

                    default:
                        return null!;
                }
            }

            [Command("all")]
            public async Task<DiscordCommandResult> ArchiveAllAsync(
                [OverrideDefault("#here")] ITextChannel? channel = null,
                bool unpinOriginals = false
            )
            {
                channel ??= Context.Channel;

                if (Context.Bot.GetChannel(SbuBotGlobals.Guild.ID, SbuBotGlobals.Channels.PIN_ARCHIVE)
                    is not ITextChannel pinArchive)
                    throw new RequiredCacheException("Could not find required pin archive channel.");

                IReadOnlyList<IUserMessage> pins = await channel.FetchPinnedMessagesAsync();

                foreach (IUserMessage message in pins.OrderBy(m => m.CreatedAt()))
                {
                    switch (Utility.TryCreatePinMessage(message))
                    {
                        case Result<LocalMessage, string>.Success pinMessage:
                            await pinArchive.SendMessageAsync(pinMessage);

                            if (unpinOriginals)
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