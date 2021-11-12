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
        [Command("ping")]
        [Description("Replies with `Pong!`.")]
        public DiscordCommandResult Ping() => Response("Pong!");

        [Command("color")]
        [Description("Replies with the given color as an embed or a random color if non is given.")]
        public DiscordCommandResult ShowColor(
            [Description("The optional color to reply with.")]
            Color? color = null
        )
        {
            color ??= Color.Random;
            return Response(new LocalEmbed().WithTitle(color.ToString()).WithColor(color));
        }

        [Command("rafael")]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [Description("Rafael.")]
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
                        .WithContent(SbuGlobals.Media.RAFAEL)
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

            await channel.SendMessageAsync(
                new LocalMessage().WithReply(message.Id).WithContent(SbuGlobals.Media.RAFAEL)
            );

            return null!;
        }
    }
}
