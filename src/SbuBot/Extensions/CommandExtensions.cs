using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Attributes;

namespace SbuBot
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CommandExtensions
    {
        public static bool IsGroup(this Module @this) => @this.Commands.GroupBy(c => c.Name).Count() == 1;

        public static IEnumerable<Command> Defaults(this Module @this)
            => @this.Commands.Where(c => c.Aliases.Count == 0);

        // TODO: make more generic for error handling
        public static string GetSignature(this Command @this, IPrefix? prefix = null)
        {
            StringBuilder builder = new(@this.FullAliases[0].Length + 16 * @this.Parameters.Count);

            if (prefix is { })
                builder.Append(prefix).Append(' ');

            builder.Append(@this.FullAliases[0]).Append(' ');

            foreach (Parameter parameter in @this.Parameters)
            {
                builder.Append(parameter.IsOptional ? '[' : '<').Append(parameter.Name);

                if (parameter.IsMultiple)
                    builder.Append(SbuGlobals.ELLIPSES);

                if (parameter.IsOptional && !parameter.IsMultiple)
                {
                    builder.Append(" = ");

                    object val = parameter.Attributes
                        .OfType<OverrideDefaultAttribute>()
                        .FirstOrDefault() is { } overrideDefault
                        ? overrideDefault.Value
                        : parameter.DefaultValue;

                    builder.Append(
                        val switch
                        {
                            null => "none",
                            { } value => value,
                        }
                    );
                }

                builder.Append(parameter.IsOptional ? ']' : '>').Append(' ');
            }

            if (builder.Length > 1)
                builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        public static string GetParameterSignature(this Command @this)
        {
            StringBuilder builder = new(@this.FullAliases[0].Length + 16 * @this.Parameters.Count);

            foreach (Parameter parameter in @this.Parameters)
            {
                builder.Append(parameter.IsOptional ? '[' : '<').Append(parameter.Name);

                if (parameter.IsMultiple)
                    builder.Append(',').Append(SbuGlobals.ELLIPSES);

                if (parameter.IsOptional)
                {
                    builder.Append(" = ");

                    object val = parameter.Attributes
                        .OfType<OverrideDefaultAttribute>()
                        .FirstOrDefault() is { } overrideDefault
                        ? overrideDefault.Value
                        : parameter.DefaultValue;

                    builder.Append(
                        val switch
                        {
                            null => "none",
                            { } value => value,
                        }
                    );
                }

                builder.Append(parameter.IsOptional ? ']' : '>').Append(' ');
            }

            if (builder.Length > 1)
                builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }
    }
}