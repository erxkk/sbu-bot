using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Qmmands;

namespace SbuBot.Commands.TypeParsers
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
                IMessage? message;

                if (value.Length < 21 && ulong.TryParse(value, out var id))
                {
                    await using (_ = context.BeginYield())
                    {
                        message = await context.Bot.FetchMessageAsync(context.ChannelId, id);
                    }

                    return message is { }
                        ? TypeParser<IMessage>.Success(message)
                        : TypeParser<IMessage>.Failure("Could not find message.");
                }

                if (SbuUtility.TryParseMessageLink(value, out var idPair))
                {
                    await using (_ = context.BeginYield())
                    {
                        message = await context.Bot.FetchMessageAsync(idPair.ChannelId, idPair.MessageId);
                    }

                    return message is { }
                        ? TypeParser<IMessage>.Success(message)
                        : TypeParser<IMessage>.Failure("Could not find message.");
                }
            }

            return TypeParser<IMessage>.Failure("Could not parse message.");
        }
    }
}