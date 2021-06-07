using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Qmmands;

namespace SbuBot.Commands.TypeParsers
{
    public sealed class UserMessageTypeParser : DiscordTypeParser<IUserMessage>
    {
        public override async ValueTask<TypeParserResult<IUserMessage>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordCommandContext context
        )
        {
            if (value.Length >= 15)
            {
                IUserMessage? message;

                if (value.Length < 21 && ulong.TryParse(value, out var id))
                {
                    await using (_ = context.BeginYield())
                    {
                        message = await context.Bot.FetchMessageAsync(context.ChannelId, id) as IUserMessage;
                    }

                    return message is { }
                        ? TypeParser<IUserMessage>.Success(message)
                        : TypeParser<IUserMessage>.Failure("Could not find message.");
                }

                if (Utility.TryParseMessageLink(value, out var idPair))
                {
                    await using (_ = context.BeginYield())
                    {
                        message = await context.Bot.FetchMessageAsync(idPair.ChannelId, idPair.MessageId)
                            as IUserMessage;
                    }

                    return message is { }
                        ? TypeParser<IUserMessage>.Success(message)
                        : TypeParser<IUserMessage>.Failure("Could not find message.");
                }
            }

            return TypeParser<IUserMessage>.Failure("Could not parse message.");
        }
    }
}