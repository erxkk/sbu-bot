using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Parsing.HelperTypes;

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
                return Success(OneOrAll<T>.All());

            if (context.Bot.Commands.GetTypeParser<T>() is not { } innerParser)
                throw new InvalidOperationException($"No inner type parser registered for {typeof(T).Name}.");

            TypeParserResult<T> innerResult = await innerParser.ParseAsync(parameter, value, context);

            return innerResult.IsSuccessful
                ? Success(OneOrAll<T>.One(innerResult.Value))
                : Failure(innerResult.FailureReason);
        }
    }
}
