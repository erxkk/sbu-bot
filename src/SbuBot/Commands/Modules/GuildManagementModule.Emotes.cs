using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Microsoft.Extensions.DependencyInjection;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Views;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Group("emote")]
        [RequireBotGuildPermissions(Permission.ManageEmojisAndStickers),
         RequireAuthorGuildPermissions(Permission.ManageEmojisAndStickers)]
        [Description("A group of commands for creating and removing emotes.")]
        public sealed class EmoteSubModule : SbuModuleBase
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
                            Discord.Cdn.GetCustomEmojiUrl(customEmoji.Id, CdnAssetFormat.Automatic),
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

            [Command("delete")]
            [Description("Removes the given emote(s) from the server.")]
            [Usage("emote remove emote", "emote remove emote1 emote2 emote3", "emote remove 855415802139901962")]
            public async Task<DiscordCommandResult> DeleteAsync(
                [Description("The emote to remove.")] IGuildEmoji emote,
                [Description("Optional additional emotes to remove.")]
                params IGuildEmoji[] additionalEmotes
            )
            {
                HashSet<IGuildEmoji> emoteSet = additionalEmotes.ToHashSet();
                emoteSet.Add(emote);

                if (emoteSet.Count > 1)
                {
                    ConfirmationState result = await ConfirmationAsync(
                        "Emote Removal",
                        "Are you sure you want to remove all those emotes?"
                    );

                    switch (result)
                    {
                        case ConfirmationState.None:
                        case ConfirmationState.Aborted:
                        case ConfirmationState.TimedOut:
                            return Reply("Aborted.");

                        case ConfirmationState.Confirmed:
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                await Task.WhenAll(emoteSet.Select(e => e.DeleteAsync()));
                return Reply("Removed all emotes.");
            }
        }
    }
}