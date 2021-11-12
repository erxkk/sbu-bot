using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Views;

namespace SbuBot.Commands.Modules
{
    public sealed partial class EmoteModule
    {
        [Group("delete")]
        [RequireBotGuildPermissions(Permission.ManageEmojisAndStickers),
         RequireAuthorGuildPermissions(Permission.ManageEmojisAndStickers)]
        [Description("Removes the given emote(s) from the server.")]
        public sealed class DeleteGroup : SbuModuleBase
        {
            [Command]
            [UsageOverride("emote remove {emote}", "emote remove {emote1} {emote2} {emote3}")]
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

            [Command]
            [UsageOverride(
                "emote remove 855415802139901962",
                "emote remove 855415802139901962 356017256481685506 215883207642447872"
            )]
            public async Task<DiscordCommandResult> DeleteFromIdsAsync(
                [Description("The id of the emote to remove.")]
                Snowflake emoteId,
                [Description("Optional additional ids of emotes to remove.")]
                params Snowflake[] additionalEmoteIds
            )
            {
                HashSet<Snowflake> emoteSet = additionalEmoteIds.ToHashSet();
                emoteSet.Add(emoteId);

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

                await Task.WhenAll(emoteSet.Select(e => Context.Guild.DeleteEmojiAsync(e)));
                return Reply("Removed all emotes.");
            }
        }
    }
}
