using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.Parsing.TypeParsers
{
    public sealed class OneOrAllTypeParser<T> : DiscordTypeParser<OneOrAll<T>>
    {
        public override async ValueTask<TypeParserResult<OneOrAll<T>>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordCommandContext context
        )
        {
            if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
                return Success(new OneOrAll<T>.All());

            TypeParserResult<T> innerResult = await context.Bot.Commands.GetTypeParser<T>()
                .ParseAsync(parameter, value, context);

            return innerResult.IsSuccessful
                ? Success(new OneOrAll<T>.Specific(innerResult.Value))
                : Failure(innerResult.FailureReason);
        }
    }
}