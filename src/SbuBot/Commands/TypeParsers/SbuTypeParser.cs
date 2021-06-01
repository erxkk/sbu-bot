using System;
using System.Threading.Tasks;

using Qmmands;

namespace SbuBot.Commands.TypeParsers
{
    public abstract class SbuTypeParser<T> : TypeParser<T>
    {
        protected abstract ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string value,
            SbuCommandContext context
        );

        public sealed override ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string value,
            CommandContext context
        )
        {
            if (context is not SbuCommandContext ctx)
                throw new InvalidOperationException($"Invalid Context type: {context.GetType().Name}");

            return ParseAsync(parameter, value, ctx);
        }
    }
}