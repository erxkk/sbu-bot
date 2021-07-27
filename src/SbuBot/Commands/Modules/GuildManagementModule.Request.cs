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
    // TODO: add abort
    public sealed partial class GuildManagementModule
    {
        [Group("request")]
        [RequireGuild(SbuGlobals.Guild.Sbu.SELF)]
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
                InteractivityExtension interactivity = Context.Bot.GetInteractivity();
                await Context.Author.GrantRoleAsync(SbuGlobals.Guild.Sbu.Role.Perm.SENATE);

                try
                {
                    MessageReceivedEventArgs waitMessageResult;
                    await Reply($"Send your message in {Mention.TextChannel(SbuGlobals.Guild.Sbu.Channel.SENATE)}.");

                    await using (_ = Context.BeginYield())
                    {
                        waitMessageResult = await interactivity.WaitForMessageAsync(
                            SbuGlobals.Guild.Sbu.Channel.SENATE,
                            e => e.Member.Id == Context.Author.Id,
                            TimeSpan.FromMinutes(3)
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            string.Format(
                                "You did not send a message in {0} in time.",
                                Mention.TextChannel(SbuGlobals.Guild.Sbu.Channel.SENATE)
                            )
                        );

                        return;
                    }

                    await waitMessageResult.Message.AddReactionAsync(
                        new LocalCustomEmoji(SbuGlobals.Guild.Emote.Vote.UP)
                    );

                    await waitMessageResult.Message.AddReactionAsync(
                        new LocalCustomEmoji(SbuGlobals.Guild.Emote.Vote.DOWN)
                    );

                    await waitMessageResult.Message.AddReactionAsync(
                        new LocalCustomEmoji(SbuGlobals.Guild.Emote.Vote.NONE)
                    );
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Guild.Sbu.Role.Perm.SENATE);
                }
            }

            [Command("quote")]
            [Cooldown(1, 1, CooldownMeasure.Hours, CooldownBucketType.Member)]
            [Description("Grants the shit-sbu-says submission role.")]
            public async Task QuoteAsync()
            {
                InteractivityExtension interactivity = Context.Bot.GetInteractivity();
                await Context.Author.GrantRoleAsync(SbuGlobals.Guild.Sbu.Role.Perm.SHIT_SBU_SAYS);

                try
                {
                    await Reply(
                        $"Send your message in {Mention.TextChannel(SbuGlobals.Guild.Sbu.Channel.Based.SHIT_SBU_SAYS)}."
                    );

                    MessageReceivedEventArgs waitMessageResult;

                    await using (_ = Context.BeginYield())
                    {
                        waitMessageResult = await interactivity.WaitForMessageAsync(
                            SbuGlobals.Guild.Sbu.Channel.Based.SHIT_SBU_SAYS,
                            e => e.Member.Id == Context.Author.Id,
                            TimeSpan.FromMinutes(3)
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            string.Format(
                                "You did not send a message in {0} in time.",
                                Mention.TextChannel(SbuGlobals.Guild.Sbu.Channel.Based.SHIT_SBU_SAYS)
                            )
                        );
                    }
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Guild.Sbu.Role.Perm.SHIT_SBU_SAYS);
                }
            }

            [Command("announce")]
            [Cooldown(1, 1, CooldownMeasure.Hours, CooldownBucketType.Member)]
            [Description("Grants the announcement submission role.")]
            public async Task AnnounceAsync()
            {
                InteractivityExtension interactivity = Context.Bot.GetInteractivity();
                await Context.Author.GrantRoleAsync(SbuGlobals.Guild.Sbu.Role.Perm.ANNOUNCEMENTS);

                try
                {
                    MessageReceivedEventArgs waitMessageResult;

                    await Reply(
                        $"Send your message in {Mention.TextChannel(SbuGlobals.Guild.Sbu.Channel.ANNOUNCEMENTS)}."
                    );

                    await using (_ = Context.BeginYield())
                    {
                        waitMessageResult = await interactivity.WaitForMessageAsync(
                            SbuGlobals.Guild.Sbu.Channel.ANNOUNCEMENTS,
                            e => e.Member.Id == Context.Author.Id,
                            TimeSpan.FromMinutes(3)
                        );
                    }

                    if (waitMessageResult is null)
                    {
                        await Reply(
                            string.Format(
                                "You did not send a message in {0} in time.",
                                Mention.TextChannel(SbuGlobals.Guild.Sbu.Channel.ANNOUNCEMENTS)
                            )
                        );
                    }
                }
                finally
                {
                    await Context.Author.RevokeRoleAsync(SbuGlobals.Guild.Sbu.Role.Perm.ANNOUNCEMENTS);
                }
            }
        }
    }
}