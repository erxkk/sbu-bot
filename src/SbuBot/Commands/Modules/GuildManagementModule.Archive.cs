using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;
using Kkommon.Exceptions;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Parsing;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Group("archive")]
        [RequireGuildConfig(SbuGuildConfig.Archive)]
        [Description("A group of commands for sending pinned messages to an archive channel.")]
        public sealed partial class ArchiveSubModule : SbuModuleBase
        {
            [Command]
            [RequireBotChannelPermissions(Permission.ManageMessages),
             RequireAuthorGuildPermissions(Permission.Administrator)]
            [Description("Archives the given message or all pinned messages in the current channel.")]
            [Remarks(
                "The Message are unpinned unless specified otherwise, specifying otherwise cannot be done when "
                + "replying to the message. In this case the message id/link/`all` must be used as message argument."
            )]
            [Usage(
                "archive (with {@reply})",
                "archive 836993360274784297",
                "archive https://discord.com/channels/732210852849123418/732231139233759324/836993360274784297",
                "archive all"
            )]
            public async Task<DiscordCommandResult> ArchiveMessageAsync(
                [OverrideDefault("{@reply}")][Description("The message that should be archived.")]
                OneOrAll<IUserMessage>? message = null,
                [Description("Whether or not the original message should be unpinned.")]
                bool unpinOriginal = true
            )
            {
                SbuGuild guild = await Context.GetDbGuildAsync();

                if (guild.ArchiveId is null)
                    return Reply("This guild doesn't have a pin archive channel set up. see `sbu help archive set`");

                ITextChannel? pinArchive = Context.Guild.GetChannel(guild.ArchiveId.Value) as ITextChannel
                    ?? await Context.Bot.FetchChannelAsync(guild.ArchiveId.Value) as ITextChannel;

                if (pinArchive is null)
                    return Reply($"Could not find required pin archive channel ({guild.ArchiveId.Value}).");

                var archivePerms = Permission.SendMessages | Permission.SendEmbeds | Permission.SendAttachments;

                if (!Context.CurrentMember.GetPermissions(pinArchive).Has(archivePerms))
                    return Reply($"I don't have the necessary permissions in the archive channel ({archivePerms:F}).");

                if (message is null)
                {
                    if (!Context.Message.ReferencedMessage.HasValue)
                        return Reply("You need to provide a message or reply to one.");

                    await PinSingleMessageAsync(Context.Message.ReferencedMessage.Value, pinArchive, unpinOriginal);
                }
                else
                {
                    switch (message)
                    {
                        case OneOrAll<IUserMessage>.Specific specific:
                        {
                            if (await PinSingleMessageAsync(specific.Value, pinArchive, unpinOriginal)
                                is Result<Unit, string>.Error error)
                                return Reply(error.Value);

                            break;
                        }

                        case OneOrAll<IUserMessage>.All:
                        {
                            IReadOnlyList<IUserMessage> pins = await Context.Channel.FetchPinnedMessagesAsync();

                            foreach (IUserMessage pinnedMessage in pins.OrderBy(m => m.CreatedAt()))
                            {
                                if (Context.Bot.StoppingToken.IsCancellationRequested)
                                    throw new OperationCanceledException();

                                if (await PinSingleMessageAsync(pinnedMessage, pinArchive, unpinOriginal)
                                    is Result<Unit, string>.Error error)
                                    return Reply(error.Value);
                            }

                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return Reply("Done.");
            }

            private async Task<Result<Unit, string>> PinSingleMessageAsync(
                IUserMessage message,
                ITextChannel channel,
                bool unpinOriginal
            )
            {
                switch (SbuUtility.TryCreatePinMessage(Context.GuildId, message))
                {
                    case Result<LocalMessage, string>.Success pinMessage:
                        await channel.SendMessageAsync(pinMessage);

                        if (unpinOriginal && message.IsPinned)
                            await message.UnpinAsync();

                        return new Result<Unit, string>.Success(new());

                    case Result<LocalMessage, string>.Error error:
                        return new Result<Unit, string>.Error(error.Value);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}