using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;
using Disqord.Rest;

using Qmmands;

namespace SbuBot.Commands.Parsing.TypeParsers
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

                if (value.Length < 21 && ulong.TryParse(value, out ulong id))
                {
                    await using (_ = context.BeginYield())
                    {
                        message = await context.Bot.FetchMessageAsync(context.ChannelId, id) as IUserMessage;
                    }

                    return message is { }
                        ? Success(message)
                        : Failure("Could not find message.");
                }

                if (SbuUtility.TryParseMessageLink(value, out (Snowflake ChannelId, Snowflake MessageId) idPair))
                {
                    await using (_ = context.BeginYield())
                    {
                        message = await context.Bot.FetchMessageAsync(idPair.ChannelId, idPair.MessageId)
                            as IUserMessage;
                    }

                    return message is { }
                        ? Success(message)
                        : Failure("Could not find message.");
                }
            }

            return Failure("Could not parse message.");
        }
    }
}