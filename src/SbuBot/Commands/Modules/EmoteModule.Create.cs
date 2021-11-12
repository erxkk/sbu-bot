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
using SbuBot.Commands.Attributes.Checks;
using SbuBot.Extensions;

namespace SbuBot.Commands.Modules
{
    public sealed partial class EmoteModule
    {
        [Command("add", "yoink")]
        [RequireBotGuildPermissions(Permission.ManageEmojisAndStickers),
         RequireAuthorGuildPermissions(Permission.ManageEmojisAndStickers)]
        [Description("Adds the given emote(s) to the server.")]
        [UsageOverride("emote add {emote}", "emote yoink {emote1} {emote2} {emote3}")]
        public async Task<DiscordCommandResult> AddAsync(
            [Description("The emote to add.")] ICustomEmoji emote,
            [Description("Optional additional emotes to add.")]
            params ICustomEmoji[] additionalEmotes
        )
        {
            HashSet<ICustomEmoji> emoteSet = additionalEmotes.ToHashSet();
            emoteSet.Add(emote);
            List<IGuildEmoji> createdEmotes = new(emoteSet.Count);

            int slots = Context.Guild.CustomEmojiSlots();

            if (Context.Guild.Emojis.Count(e => !e.Value.IsAnimated) + emoteSet.Count(e => !e.IsAnimated) > slots)
                return Reply("This would exceed the maximum amount of regular emotes.");

            if (Context.Guild.Emojis.Count(e => e.Value.IsAnimated) + emoteSet.Count(e => e.IsAnimated) > slots)
                return Reply("This would exceed the maximum amount of animated emotes.");

            HttpClient client = Context.Services.GetRequiredService<HttpClient>();

            MemoryStream uploadBuffer = new();

            foreach (ICustomEmoji customEmoji in emoteSet)
            {
                CancellationTokenSource cts = CancellationTokenSource
                    .CreateLinkedTokenSource(Context.Bot.StoppingToken);

                cts.CancelAfter(TimeSpan.FromSeconds(5));

                try
                {
                    Stream currentStream = await client.GetStreamAsync(
                        customEmoji.GetUrl(CdnAssetFormat.Automatic),
                        cts.Token
                    );

                    await currentStream.CopyToAsync(uploadBuffer, cts.Token);
                    uploadBuffer.Position = 0;

                    createdEmotes.Add(
                        await Context.Guild.CreateEmojiAsync(
                            customEmoji.Name,
                            uploadBuffer,
                            cancellationToken: cts.Token
                        )
                    );
                }
                catch (OperationCanceledException)
                {
                    if (Context.Bot.StoppingToken.IsCancellationRequested)
                        throw;
                }
                finally
                {
                    await uploadBuffer.FlushAsync(cts.Token);
                    uploadBuffer.Position = 0;
                }
            }

            if (emoteSet.Count > createdEmotes.Count)
            {
                emoteSet.ExceptWith(createdEmotes);

                await Response(
                    new LocalEmbed()
                        .WithTitle($"Download failed for {emoteSet.Count} emotes.")
                        .WithDescription(string.Join(", ", emoteSet.Select(e => $"`{e.Name} {e.Id}`")))
                );
            }

            return Response(
                new LocalEmbed()
                    .WithTitle($"Added {createdEmotes.Count} emotes.")
                    .WithDescription(string.Join("", createdEmotes.Select(e => e.Tag)))
            );
        }

        [Group("create")]
        [RequireBotGuildPermissions(Permission.ManageEmojisAndStickers),
         RequireAuthorGuildPermissions(Permission.ManageEmojisAndStickers)]
        [Description("Adds the given emote(s) to the server.")]
        public sealed class CreateGroup : SbuModuleBase
        {
            [Command]
            [RequireAttachment]
            [UsageOverride("emote create", "emote mk NewEmote")]
            public async Task<DiscordCommandResult> CreateFromAttachmentAsync(
                [Range(3, 32, true, true)]
                [Description("The optional name of the new emote.")]
                [Remarks("Must be between 3 and 32 characters long.")]
                string? name = null
            )
            {
                name = name?.Replace(' ', '_');
                name ??= "newEmote";

                IAttachment attachment = Context.Message.Attachments[0];
                HttpClient client = Context.Services.GetRequiredService<HttpClient>();

                if (attachment.FileSize > 256)
                    return Reply("The file is too large, it cannot exceed a maximum of 256 bytes.");

                // TODO: check if gifv is video/* type and valid to upload as gif (tenor/giphy urls might expose mp4 instead)
                if (attachment.ContentType is not ("image/png" or "image/jpeg" or "image/jpg" or "video/gifv"))
                    return Reply($"Invalid or unhandled file type `{attachment.ContentType ?? "unknown"}`.");

                CancellationTokenSource cts = CancellationTokenSource
                    .CreateLinkedTokenSource(Context.Bot.StoppingToken);

                IGuildEmoji emote;

                await using (Stream imageStream = await client.GetStreamAsync(attachment.Url, cts.Token))
                {
                    MemoryStream uploadBuffer = new();

                    await imageStream.CopyToAsync(uploadBuffer, cts.Token);
                    uploadBuffer.Position = 0;

                    emote = await Context.Guild.CreateEmojiAsync(name, uploadBuffer, cancellationToken: cts.Token);
                }

                return Response($"Created emote: {emote.Tag}");
            }

            [Command]
            [UsageOverride("emote create https://...", "emote mk https://... NewEmote")]
            public async Task<DiscordCommandResult> CreateFromAttachmentAsync(
                [Description("The image url of the new emote.")]
                Uri url,
                [Range(3, 32, true, true)]
                [Description("The optional name of the new emote.")]
                [Remarks("Must be between 3 and 32 characters long.")]
                string? name = null
            )
            {
                name = name?.Replace(' ', '_');
                name ??= "newEmote";

                HttpClient client = Context.Services.GetRequiredService<HttpClient>();

                CancellationTokenSource cts = CancellationTokenSource
                    .CreateLinkedTokenSource(Context.Bot.StoppingToken);

                var response = await client.SendAsync(new(HttpMethod.Head, url), cts.Token);
                var mediaType = response.Content.Headers.ContentType?.MediaType;

                if (mediaType is null)
                    return Reply("The server didn't answer with with the `Content-Type` header.");

                if (mediaType.StartsWith("image"))
                    return Reply("The url doesn't point to an image.");

                if (response.Content.Headers.ContentLength is > 256)
                    return Reply("The image size is too large.");

                IGuildEmoji emote;

                // cancel if download takes too long
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                await using (Stream imageStream = await client.GetStreamAsync(url, cts.Token))
                {
                    MemoryStream uploadBuffer = new();

                    await imageStream.CopyToAsync(uploadBuffer, cts.Token);
                    uploadBuffer.Position = 0;

                    emote = await Context.Guild.CreateEmojiAsync(name, uploadBuffer, cancellationToken: cts.Token);
                }

                return Response($"Created emote: {emote.Tag}");
            }
        }
    }
}
