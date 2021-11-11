using System;
using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

using Qmmands;

using SbuBot.Commands.Attributes;

namespace SbuBot.Commands.Modules
{
    [Description("A collection of utility commands.")]
    public sealed class UtilityModule : SbuModuleBase
    {
        public const string RAFAEL
            = "https://cdn.discordapp.com/attachments/908422577456828436/908449232711716914/unknown.png";

        [Command("ping")]
        [Description("Replies with `Pong!`.")]
        [Usage("ping")]
        public DiscordCommandResult Ping() => Reply("Pong!");

        [Command("color")]
        [Description("Replies with the given color as an embed or a random color if non is given.")]
        [Usage("color green", "color #afafaf")]
        public DiscordCommandResult ShowColor(
            [Description("The optional color to reply with.")]
            Color? color = null
        )
        {
            color ??= Color.Random;
            return Reply(new LocalEmbed().WithTitle(color.ToString()).WithColor(color));
        }

        [Command("rafael")]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [Description("Rafael.")]
        [Usage(
            "rafael (with {@reply})",
            "rafael 836993360274784297",
            "rafael https://discord.com/channels/732210852849123418/732231139233759324/836993360274784297"
        )]
        public async Task<DiscordCommandResult> RafaelAsync(
            [OverrideDefault("{@reply}")][Description("Rafael.")]
            IUserMessage? message = null
        )
        {
            if (message is null)
            {
                if (Context.Message.Reference?.MessageId is null)
                    return Reply("You need to provide a message or reply to one.");

                await Context.Message.DeleteAsync();

                return Response(
                    new LocalMessage().WithReply(Context.Message.Reference.MessageId.Value)
                        .WithContent(UtilityModule.RAFAEL)
                );
            }

            if (Context.Guild.GetChannel(message.ChannelId) is not ITextChannel channel)
            {
                return Reply(
                    string.Format(
                        "Couldn't find the channel this message was supposed to be in ({0}).",
                        Mention.Channel(message.ChannelId)
                    )
                );
            }

            await Context.Message.DeleteAsync();
            await channel.SendMessageAsync(new LocalMessage().WithReply(message.Id).WithContent(UtilityModule.RAFAEL));
            return null!;
        }
    }
}