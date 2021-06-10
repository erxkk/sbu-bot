using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;

using Kkommon;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Checks;
using SbuBot.Commands.Information;

namespace SbuBot.Commands.Modules
{
    [Description("A collection of commands for server management like pin-archival or emote creation.")]
    public sealed class GuildManagementModule : SbuModuleBase
    {
        [Group("archive"), RequirePinBrigade(Group = "AdminOrPinBrigade"), RequireAdmin(Group = "AdminOrPinBrigade")]
        [Description("A group of commands for archiving messages.")]
        public sealed class ArchiveGroup : SbuModuleBase
        {
            [Command]
            [Description("Archives the given message directly.")]
            [Remarks(
                "The Message are unpinned unless specified otherwise, specifying otherwise cannot be done when "
                + "replying to the message. In this case the message id/link must be used as message argument."
            )]
            public async Task<DiscordCommandResult> ArchiveMessageAsync(
                [OverrideDefault("@reply")][Description("The message that should be archived.")]
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

                if (Context.Bot.GetChannel(SbuGlobals.Guild.SELF, SbuGlobals.Channel.Based.PIN_ARCHIVE)
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
                [OverrideDefault("#here")][Description("The channel of which the pins should be archived.")]
                ITextChannel? channel = null,
                [Description("Whether or not the original messages should be unpinned.")]
                bool unpinOriginals = true
            )
            {
                channel ??= Context.Channel;

                if (Context.Bot.GetChannel(SbuGlobals.Guild.SELF, SbuGlobals.Channel.Based.PIN_ARCHIVE)
                    is not ITextChannel pinArchive)
                    throw new RequiredCacheException("Could not find required pin archive channel.");

                IReadOnlyList<IUserMessage> pins = await channel.FetchPinnedMessagesAsync();

                foreach (IUserMessage message in pins.OrderBy(m => m.CreatedAt()))
                {
                    if (Context.Bot.StoppingToken.IsCancellationRequested)
                        throw new OperationCanceledException();

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

        // TODO: TEST
        [Group("emote")]
        [Description("A group of commands for creating and removing emotes.")]
        public sealed class EmoteGroup : SbuModuleBase
        {
            [Command("add")]
            [Description("Adds the given emote(s) to the server.")]
            public async Task<DiscordCommandResult> AddAsync(
                [Description("The emote to add.")] ICustomEmoji emote,
                [Description("Optional additional emotes to add.")]
                params ICustomEmoji[] additionalEmotes
            )
            {
                HashSet<ICustomEmoji> emoteSet = additionalEmotes.ToHashSet();
                emoteSet.Add(emote);

                int slots = Utility.CustomEmojiSlots(Context.Guild);

                if (Context.Guild.Emojis.Count(e => !e.Value.IsAnimated) + emoteSet.Count(e => !e.IsAnimated) > slots)
                    return Reply("This would exceed the maximum amount of emotes.");

                if (Context.Guild.Emojis.Count(e => e.Value.IsAnimated) + emoteSet.Count(e => e.IsAnimated) > slots)
                    return Reply("This would exceed the maximum amount of animated emotes.");

                HttpClient client = Context.Services.GetRequiredService<HttpClient>();

                int downloaded = 0;
                MemoryStream uploadBuffer = new();

                foreach (ICustomEmoji customEmoji in emoteSet)
                {
                    CancellationTokenSource cts = CancellationTokenSource
                        .CreateLinkedTokenSource(Context.Bot.StoppingToken);

                    cts.CancelAfter(5 * 1000);

                    try
                    {
                        Stream currentStream = await client.GetStreamAsync(
                            Discord.Cdn.GetCustomEmojiUrl(customEmoji.Id, customEmoji.IsAnimated),
                            cts.Token
                        );

                        downloaded++;

                        await currentStream.CopyToAsync(uploadBuffer, cts.Token);
                        await Context.Guild.CreateEmojiAsync(customEmoji.Name, uploadBuffer);
                    }
                    catch (OperationCanceledException)
                    {
                        if (Context.Bot.StoppingToken.IsCancellationRequested)
                            throw;
                    }
                    finally
                    {
                        await uploadBuffer.FlushAsync(Context.Bot.StoppingToken);
                        uploadBuffer.Position = 0;
                    }
                }

                if (emoteSet.Count > downloaded)
                    await Reply($"Download failed for {emoteSet.Count - downloaded} emotes.");

                return Reply("Added all emotes.");
            }

            [Command("remove", "delete")]
            [Description("Removes the given emote(s) from the server.")]
            public async Task<DiscordCommandResult> RemoveAsync(
                [Description("The emote to remove.")] IGuildEmoji emote,
                [Description("Optional additional emotes to remove.")]
                params IGuildEmoji[] additionalEmotes
            )
            {
                HashSet<IGuildEmoji> emoteSet = additionalEmotes.ToHashSet();
                emoteSet.Add(emote);

                if (emoteSet.Count > 1)
                    await Reply("Are you sure you want to remove all those emotes? Respond `yes` to confirm.");

                MessageReceivedEventArgs waitConfirmResult;

                await using (_ = Context.BeginYield())
                {
                    waitConfirmResult = await Context.WaitForMessageAsync(
                        e => e.Member.Id == Context.Author.Id,
                        cancellationToken: Context.Bot.StoppingToken
                    );
                }

                if (waitConfirmResult is null
                    || !waitConfirmResult.Message.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    return Reply("Aborted.");

                foreach (IGuildEmoji customEmoji in emoteSet)
                    await customEmoji.DeleteAsync();

                return Reply("Removed all emotes.");
            }
        }
    }
}