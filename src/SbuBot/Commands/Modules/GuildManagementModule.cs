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
    [Description("A collection of commands for server management like pin-archival or emote creation.")]
    public sealed class GuildManagementModule : SbuModuleBase
    {
        [Group("archive"), RequireAuthorRole(SbuBotGlobals.Roles.PIN_BRIGADE, Group = "AdminOrPinBrigade"),
         RequireAuthorAdmin(Group = "AdminOrPinBrigade")]
        [Description("A group of commands for archiving messages.")]
        public sealed class PinGroup : SbuModuleBase
        {
            [Command]
            [Description("Archives the given message directly.")]
            [Remarks(
                "The Message are unpinned unless specified otherwise, specifying otherwise cannot be done when "
                + "replying to the message. In this case the message id/link must be used as message argument."
            )]
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
            [Description(
                "Archives all pinned messages in the given channel, or in the channel this command is used in if no "
                + "channel is specified."
            )]
            [Remarks(
                "Messages are unpinned unless specified otherwise, specifying otherwise cannot be done without "
                + "specifying the channel. In this case the channel id/mention/name must be used as message argument."
            )]
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