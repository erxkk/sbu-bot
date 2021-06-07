using System;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

namespace SbuBot.Commands.TypeParsers
{
    public abstract class SbuTypeParserBase<T> : DiscordTypeParser<T>
    {
        protected abstract ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        );

        public sealed override ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordCommandContext context
        )
        {
            if (context is not SbuCommandContext ctx)
                throw new InvalidOperationException($"Invalid Context type: {context.GetType().Name}");

            return ParseAsync(parameter, value, ctx);
        }
    }
}