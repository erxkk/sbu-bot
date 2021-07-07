using System;
using System.Linq;
using System.Threading.Tasks;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.TypeParsers.Descriptors;

namespace SbuBot.Commands.TypeParsers
{
    public abstract class DescriptorTypeParserBase<T> : DiscordGuildTypeParser<T> where T : struct, IDescriptor
    {
        protected abstract ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string[] values,
            DiscordGuildCommandContext context
        );

        // TODO: ignore escaped separator (or don't, who tf uses "::" in a normal sentence?)
        public sealed override ValueTask<TypeParserResult<T>> ParseAsync(
            Parameter parameter,
            string value,
            DiscordGuildCommandContext context
        ) => ParseAsync(
            parameter,
            value.Split(
                    SbuGlobals.DESCRIPTOR_SEPARATOR,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                )
                .ToArray(),
            context
        );
    }
}