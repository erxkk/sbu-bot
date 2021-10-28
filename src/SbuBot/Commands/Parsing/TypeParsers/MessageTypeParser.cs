using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Qmmands;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class MessageTypeParser : DiscordTypeParser<IMessage>
    {
        public override async ValueTask<TypeParserResult<IMessage>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordCommandContext context
        )
        {
            if (value.Length >= 15)
            {
                if (value.Length < 21 && ulong.TryParse(value, out var id))
                {
                    return await context.Bot.FetchMessageAsync(context.ChannelId, id) is { } message
                        ? Success(message)
                        : Failure("Could not find message.");
                }

                if (SbuUtility.TryParseMessageLink(value, out var idPair))
                {
                    return await context.Bot.FetchMessageAsync(idPair.ChannelId, idPair.MessageId) is { } message
                        ? Success(message)
                        : Failure("Could not find message.");
                }
            }

            return Failure("Could not parse message.");
        }
    }
}