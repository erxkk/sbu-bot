using System.Threading.Tasks;

using Disqord;
using Disqord.Bot;

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
            TypeParser<IMessage> typeParser = context.Bot.Commands.GetTypeParser<IMessage>();
            TypeParserResult<IMessage> result = await typeParser.ParseAsync(parameter, value, context);

            if (!result.IsSuccessful)
                return Failure(result.FailureReason);

            if (result.Value is IUserMessage userMessage)
                return Success(userMessage);

            return Failure("The message was not a message sent by a user.");
        }
    }
}
