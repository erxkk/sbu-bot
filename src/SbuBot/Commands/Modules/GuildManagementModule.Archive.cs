using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Commands.Parsing.HelperTypes;
using SbuBot.Extensions;
using SbuBot.Models;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Group("archive")]
        [RequireGuildConfig(SbuGuildConfig.Archive)]
        [Description("A group of commands for sending pinned messages to an archive channel.")]
        public sealed class ArchiveSubModule : SbuModuleBase
        {
            [Command]
            [RequireBotChannelPermissions(Permission.ManageMessages),
             RequireAuthorGuildPermissions(Permission.ManageMessages)]
            [Description("Archives the given message or all pinned messages in the current channel.")]
            [Remarks(
                "The Message are unpinned unless specified otherwise, specifying otherwise cannot be done when "
                + "replying to the message. In this case the message id/link/`all` must be used as message argument."
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
                    ?? await Bot.FetchChannelAsync(guild.ArchiveId.Value) as ITextChannel;

                if (pinArchive is null)
                    return Reply($"Could not find required pin archive channel ({guild.ArchiveId.Value}).");

                const Permission archivePerms = Permission.SendMessages
                    | Permission.SendEmbeds
                    | Permission.SendAttachments;

                if (!Context.CurrentMember.GetPermissions(pinArchive).Has(archivePerms))
                    return Reply($"I don't have the necessary permissions in the archive channel ({archivePerms:F}).");

                if (message is null)
                {
                    if (Context.Message.Reference?.MessageId is null)
                        return Reply("You need to provide a message or reply to one.");

                    IMessage fetchedMessage = await Context.Channel
                        .FetchMessageAsync(Context.Message.Reference.MessageId.Value);

                    if (fetchedMessage is not IUserMessage referencedMessage)
                        return Reply("Could not find message.");

                    await PinSingleMessageAsync(referencedMessage, pinArchive, unpinOriginal);
                }
                else
                {
                    if (message.IsAll)
                    {
                        IReadOnlyList<IUserMessage> pins = await Context.Channel.FetchPinnedMessagesAsync();

                        foreach (IUserMessage pinnedMessage in pins.OrderBy(m => m.CreatedAt()))
                        {
                            if (Bot.StoppingToken.IsCancellationRequested)
                                throw new OperationCanceledException();

                            if (await PinSingleMessageAsync(pinnedMessage, pinArchive, unpinOriginal)
                                is Result<Unit, string>.Error singleError)
                                return Reply(singleError.Value);
                        }
                    }
                    else
                    {
                        if (await PinSingleMessageAsync(message.Value, pinArchive, unpinOriginal)
                            is Result<Unit, string>.Error error)
                            return Reply(error.Value);
                    }
                }

                return Response("Archived.");
            }

            [Command("set")]
            [RequireAuthorGuildPermissions(Permission.Administrator)]
            [Description("Sets the current pin archive.")]
            [UsageOverride("archive set #channel", "archive set 836993360274784297")]
            public async Task<DiscordCommandResult> SetArchiveAsync(
                [Description("The channel that should be the new archive.")]
                ITextChannel archive
            )
            {
                SbuDbContext context = Context.GetSbuDbContext();

                SbuGuild? guild = await context.GetGuildAsync(Context.Guild);
                guild!.ArchiveId = archive.Id;
                await context.SaveChangesAsync();

                return Response($"{archive} is now the pin archive.");
            }

            [Command("list")]
            [Description("Lists the current pin archive.")]
            [UsageOverride("archive list")]
            public async Task<DiscordCommandResult> GetArchiveAsync()
            {
                SbuGuild guild = await Context.GetDbGuildAsync();

                return Response(
                    guild.ArchiveId is null
                        ? "This guild doesn't have a pin archive channel set up. see `sbu help archive set`"
                        : $"{Mention.Channel(guild.ArchiveId.Value)} is the pin archive."
                );
            }

            private async Task<Result<Unit, string>> PinSingleMessageAsync(
                IUserMessage message,
                ITextChannel channel,
                bool unpinOriginal
            )
            {
                switch (SbuUtility.TryCreatePinMessage(Context.GuildId, message))
                {
                    case Result<(LocalMessage?, LocalMessage), string>.Success pinMessage:
                    {
                        if (pinMessage.Value.Item1 is { })
                            await channel.SendMessageAsync(pinMessage.Value.Item1);

                        await channel.SendMessageAsync(pinMessage.Value.Item2);

                        if (unpinOriginal && message.IsPinned)
                            await message.UnpinAsync();

                        return new Result<Unit, string>.Success(new());
                    }

                    case Result<(LocalMessage?, LocalMessage), string>.Error error:
                        return new Result<Unit, string>.Error(error.Value);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
