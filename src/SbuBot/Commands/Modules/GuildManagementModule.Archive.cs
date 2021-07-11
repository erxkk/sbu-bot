using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Exceptions;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Group("archive")]
        [RequirePinBrigade(Group = "AdminOrPinBrigade"), RequireAdmin(Group = "AdminOrPinBrigade"),
         RequireGuild(SbuGlobals.Guild.Sbu.SELF)]
        [Description("A group of commands for archiving messages.")]
        public sealed class ArchiveSubModule : SbuModuleBase
        {
            [Command]
            [Description("Archives the given message directly.")]
            [Remarks(
                "The Message are unpinned unless specified otherwise, specifying otherwise cannot be done when "
                + "replying to the message. In this case the message id/link must be used as message argument."
            )]
            [Usage(
                "archive (with {@reply})",
                "archive 836993360274784297",
                "archive https://discord.com/channels/732210852849123418/732231139233759324/836993360274784297"
            )]
            public async Task<DiscordCommandResult> ArchiveMessageAsync(
                [OverrideDefault("{@reply}")][Description("The message that should be archived.")]
                IUserMessage? message = null,
                [Description("Whether or not the original message should be unpinned.")]
                bool unpinOriginal = true
            )
            {
                if (message is null)
                {
                    if (!Context.Message.ReferencedMessage.HasValue)
                        return Reply("You need to provide a message or reply to one.");

                    message = Context.Message.ReferencedMessage.Value;
                }

                if (Context.Bot.GetPinArchive() is not ITextChannel pinArchive)
                    throw new NotCachedException("Could not find required pin archive channel.");

                switch (SbuUtility.TryCreatePinMessage(message))
                {
                    case Result<LocalMessage, string>.Success pinMessage:
                        await pinArchive.SendMessageAsync(pinMessage);

                        if (unpinOriginal && message.IsPinned)
                            await message.UnpinAsync();

                        return Reply("Done.");

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
            [Usage("archive all", "archive all #channel", "archive all 732211844315349005")]
            public async Task<DiscordCommandResult> ArchiveAllAsync(
                [OverrideDefault("{#here}")][Description("The channel of which the pins should be archived.")]
                ITextChannel? channel = null,
                [Description("Whether or not the original messages should be unpinned.")]
                bool unpinOriginals = true
            )
            {
                channel ??= Context.Channel;

                if (Context.Bot.GetPinArchive() is not ITextChannel pinArchive)
                    throw new NotCachedException("Could not find required pin archive channel.");

                IReadOnlyList<IUserMessage> pins = await channel.FetchPinnedMessagesAsync();

                foreach (IUserMessage message in pins.OrderBy(m => m.CreatedAt()))
                {
                    if (Context.Bot.StoppingToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    switch (SbuUtility.TryCreatePinMessage(message))
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

                return Reply("Done.");
            }
        }
    }
}