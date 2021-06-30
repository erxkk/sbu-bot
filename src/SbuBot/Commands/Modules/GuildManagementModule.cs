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

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Exceptions;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    // TODO: Usage View
    [Description("A collection of commands for server management like pin-archival or emote creation.")]
    public sealed class GuildManagementModule : SbuModuleBase
    {
        [Group("archive"), RequirePinBrigade(Group = "AdminOrPinBrigade"), RequireAdmin(Group = "AdminOrPinBrigade"),
         RequireGuild(SbuGlobals.Guild.SELF)]
        [Description("A group of commands for archiving messages.")]
        public sealed class ArchiveGroup : SbuModuleBase
        {
            [Command]
            [Description("Archives the given message directly.")]
            [Remarks(
                "The Message are unpinned unless specified otherwise, specifying otherwise cannot be done when "
                + "replying to the message. In this case the message id/link must be used as message argument."
            )]
            [Usage(
                "archive (with reply)",
                "archive 836993360274784297",
                "archive https://discord.com/channels/732210852849123418/732231139233759324/836993360274784297"
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
                    throw new NotCachedException("Could not find required pin archive channel.");

                switch (SbuUtility.TryCreatePinMessage(message))
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
            [Usage("archive all", "archive all #channel", "archive all 732211844315349005")]
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

                return Reply("Finished.");
            }
        }

        [Group("request")]
        [Description("A group of commands for requesting access to restricted channels or permissions.")]
        public sealed class RequestGroup : SbuModuleBase
        {
            [Command("vote"), Cooldown(1, 1, CooldownMeasure.Hours, CooldownBucketType.Member)]
            [Description(
                "Grants the senate submission role and waits to automatically add vote emotes to the next message."
            )]
            public async Task VoteAsync()
            {
                InteractivityExtension interactivity = Context.Bot.GetInteractivity();
                await Context.Author.GrantRoleAsync(SbuGlobals.Role.Perm.SENATE);

                try
                {
                    MessageReceivedEventArgs waitMessageResult;
                    await Reply($"Send your message in {Mention.TextChannel(SbuGlobals.Channel.SENATE)}.");

                    await using (_ = Context.BeginYield())
                    {
                        waitMessageResult = await interactivity.WaitForMessageAsync(
                            SbuGlobals.Channel.SENATE,
                            e => e.Member.Id == Context.Author.Id
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            $"You did not send a message in {Mention.TextChannel(SbuGlobals.Channel.SENATE)} in time."
                        );

                        return;
                    }

                    await waitMessageResult.Message.AddReactionAsync(new LocalCustomEmoji(SbuGlobals.Emote.Vote.UP));
                    await waitMessageResult.Message.AddReactionAsync(new LocalCustomEmoji(SbuGlobals.Emote.Vote.DOWN));
                    await waitMessageResult.Message.AddReactionAsync(new LocalCustomEmoji(SbuGlobals.Emote.Vote.NONE));
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Role.Perm.SENATE);
                }
            }

            [Command("quote"), Cooldown(1, 1, CooldownMeasure.Hours, CooldownBucketType.Member)]
            [Description("Grants the shit-sbu-says submission role.")]
            public async Task QuoteAsync()
            {
                InteractivityExtension interactivity = Context.Bot.GetInteractivity();
                await Context.Author.GrantRoleAsync(SbuGlobals.Role.Perm.SHIT_SBU_SAYS);

                try
                {
                    await Reply($"Send your message in {Mention.TextChannel(SbuGlobals.Channel.Based.SHIT_SBU_SAYS)}.");
                    MessageReceivedEventArgs waitMessageResult;

                    await using (_ = Context.BeginYield())
                    {
                        waitMessageResult = await interactivity.WaitForMessageAsync(
                            SbuGlobals.Channel.Based.SHIT_SBU_SAYS,
                            e => e.Member.Id == Context.Author.Id
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            string.Format(
                                "You did not send a message in {0} in time.",
                                Mention.TextChannel(SbuGlobals.Channel.Based.SHIT_SBU_SAYS)
                            )
                        );
                    }
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Role.Perm.SHIT_SBU_SAYS);
                }
            }

            [Command("announce"), Cooldown(1, 1, CooldownMeasure.Hours, CooldownBucketType.Member)]
            [Description("Grants the announcement submission role.")]
            public async Task AnnounceAsync()
            {
                InteractivityExtension interactivity = Context.Bot.GetInteractivity();
                await Context.Author.GrantRoleAsync(SbuGlobals.Role.Perm.ANNOUNCEMENTS);

                try
                {
                    MessageReceivedEventArgs waitMessageResult;
                    await Reply($"Send your message in {Mention.TextChannel(SbuGlobals.Channel.ANNOUNCEMENTS)}.");

                    await using (_ = Context.BeginYield())
                    {
                        waitMessageResult = await interactivity.WaitForMessageAsync(
                            SbuGlobals.Channel.ANNOUNCEMENTS,
                            e => e.Member.Id == Context.Author.Id
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            string.Format(
                                "You did not send a message in {0} in time.",
                                Mention.TextChannel(SbuGlobals.Channel.ANNOUNCEMENTS)
                            )
                        );
                    }
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Role.Perm.ANNOUNCEMENTS);
                }
            }
        }

        [Group("emote"), RequireBotGuildPermissions(Permission.ManageEmojis),
         RequireAuthorGuildPermissions(Permission.ManageEmojis)]
        [Description("A group of commands for creating and removing emotes.")]
        public sealed class EmoteGroup : SbuModuleBase
        {
            [Command("add")]
            [Description("Adds the given emote(s) to the server.")]
            [Usage("emote add emote", "emote add emote1 emote2 emote3")]
            public async Task<DiscordCommandResult> AddAsync(
                [Description("The emote to add.")] ICustomEmoji emote,
                [Description("Optional additional emotes to add.")]
                params ICustomEmoji[] additionalEmotes
            )
            {
                HashSet<ICustomEmoji> emoteSet = additionalEmotes.ToHashSet();
                emoteSet.Add(emote);

                int slots = Context.Guild.CustomEmojiSlots();

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
                        uploadBuffer.Position = 0;
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
            [Usage("emote remove emote", "emote remove emote1 emote2 emote3", "emote remove 855415802139901962")]
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