using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

namespace SbuBot.Commands.Modules
{
    public sealed partial class GuildManagementModule
    {
        [Group("request")]
        [RequireGuild(SbuGlobals.Guild.SBU)]
        [Description("A group of commands for requesting access to restricted channels or permissions.")]
        public sealed class RequestSubModule : SbuModuleBase
        {
            [Command("vote")]
            [Cooldown(1, 1, CooldownMeasure.Hours, CooldownBucketType.Member)]
            [Description(
                "Grants the senate submission role and waits to automatically add vote emotes to the next message."
            )]
            public async Task VoteAsync()
            {
                await Context.Author.GrantRoleAsync(SbuGlobals.Role.SENATE);

                try
                {
                    MessageReceivedEventArgs waitMessageResult;
                    await Response($"Send your message in {Mention.Channel(SbuGlobals.Channel.SENATE)}.");

                    await using (Context.BeginYield())
                    {
                        waitMessageResult = await await Task.WhenAny(
                            new[]
                            {
                                Bot.WaitForMessageAsync(
                                    SbuGlobals.Channel.SENATE,
                                    e => e.Member.Id == Context.Author.Id,
                                    TimeSpan.FromMinutes(3)
                                ),
                                Context.WaitForMessageAsync(
                                    e => e.Message.Content.Equals("abort", StringComparison.OrdinalIgnoreCase),
                                    TimeSpan.FromMinutes(3)
                                ),
                            }
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            string.Format(
                                "You did not send a message in {0} in time.",
                                Mention.Channel(SbuGlobals.Channel.SENATE)
                            )
                        );

                        return;
                    }
                    else if (waitMessageResult.Message.Content.Equals("abort", StringComparison.OrdinalIgnoreCase))
                    {
                        await Reply("Aborted.");
                        return;
                    }

                    await waitMessageResult.Message.AddReactionAsync(new LocalCustomEmoji(SbuGlobals.Emote.Vote.UP));
                    await waitMessageResult.Message.AddReactionAsync(new LocalCustomEmoji(SbuGlobals.Emote.Vote.DOWN));
                    await waitMessageResult.Message.AddReactionAsync(new LocalCustomEmoji(SbuGlobals.Emote.Vote.NONE));
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Role.SENATE);
                }
            }

            [Command("quote")]
            [Cooldown(1, 1, CooldownMeasure.Hours, CooldownBucketType.Member)]
            [Description("Grants the shit-sbu-says submission role.")]
            public async Task QuoteAsync()
            {
                await Context.Author.GrantRoleAsync(SbuGlobals.Role.SHIT_SBU_SAYS);

                try
                {
                    await Response($"Send your message in {Mention.Channel(SbuGlobals.Channel.SHIT_SBU_SAYS)}.");
                    MessageReceivedEventArgs waitMessageResult;

                    await using (Context.BeginYield())
                    {
                        waitMessageResult = await await Task.WhenAny(
                            new[]
                            {
                                Bot.WaitForMessageAsync(
                                    SbuGlobals.Channel.SHIT_SBU_SAYS,
                                    e => e.Member.Id == Context.Author.Id,
                                    TimeSpan.FromMinutes(3)
                                ),
                                Context.WaitForMessageAsync(
                                    e => e.Message.Content.Equals("abort", StringComparison.OrdinalIgnoreCase),
                                    TimeSpan.FromMinutes(3)
                                ),
                            }
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            string.Format(
                                "You did not send a message in {0} in time.",
                                Mention.Channel(SbuGlobals.Channel.SHIT_SBU_SAYS)
                            )
                        );
                    }
                    else if (waitMessageResult.Message.Content.Equals("abort", StringComparison.OrdinalIgnoreCase))
                    {
                        await Reply("Aborted.");
                    }
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Role.SHIT_SBU_SAYS);
                }
            }

            [Command("announce")]
            [Cooldown(1, 1, CooldownMeasure.Hours, CooldownBucketType.Member)]
            [Description("Grants the announcement submission role.")]
            public async Task AnnounceAsync()
            {
                await Context.Author.GrantRoleAsync(SbuGlobals.Role.ANNOUNCEMENTS);

                try
                {
                    MessageReceivedEventArgs waitMessageResult;
                    await Response($"Send your message in {Mention.Channel(SbuGlobals.Channel.ANNOUNCEMENTS)}.");

                    await using (Context.BeginYield())
                    {
                        waitMessageResult = await await Task.WhenAny(
                            new[]
                            {
                                Bot.WaitForMessageAsync(
                                    SbuGlobals.Channel.ANNOUNCEMENTS,
                                    e => e.Member.Id == Context.Author.Id,
                                    TimeSpan.FromMinutes(3)
                                ),
                                Context.WaitForMessageAsync(
                                    e => e.Message.Content.Equals("abort", StringComparison.OrdinalIgnoreCase),
                                    TimeSpan.FromMinutes(3)
                                ),
                            }
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            string.Format(
                                "You did not send a message in {0} in time.",
                                Mention.Channel(SbuGlobals.Channel.ANNOUNCEMENTS)
                            )
                        );
                    }
                    else if (waitMessageResult.Message.Content.Equals("abort", StringComparison.OrdinalIgnoreCase))
                    {
                        await Reply("Aborted.");
                    }
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Role.ANNOUNCEMENTS);
                }
            }
        }
    }
}
