using System.ComponentModel;
using System.Linq;
using System.Text;

using Disqord.Bot;

using Qmmands;

using SbuBot.Commands.Information;

namespace SbuBot
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CommandExtensions
    {
        // overloads of a command grouped together
        public static bool IsOverloadGroup(this Module @this)
            => @this.Submodules.Count == 0 && @this.Commands.All(c => c.Aliases.Count == 0);

        // will also apply for command overload groups
        public static bool IsGroupDefault(this Command @this) => @this.Aliases.Count == 0;

        public static string GetSignature(this Parameter @this)
        {
            StringBuilder builder = new(16);
            builder.Append(@this.IsOptional ? '[' : '<').Append(@this.Name);

            if (@this.IsMultiple)
                builder.Append('…');

            if (@this.IsOptional)
            {
                builder.Append(" = ");

                object val = @this.Attributes
                    .OfType<OverrideDefaultAttribute>()
                    .FirstOrDefault() is { } overrideDefault
                    ? overrideDefault.Value
                    : @this.DefaultValue;

                builder.Append(
                    val switch
                    {
                        "you" => "you",
                        string str => $"\"{str}\"",
                        null => "none",
                        _ => @this.DefaultValue,
                    }
                );

                builder.Append(@this.IsOptional ? ']' : '>');
            }

            return builder.ToString();
        }

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
                    builder.Append(',').Append('…');

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
                            _ => parameter.DefaultValue,
                        }
                    );
                }

                builder.Append(parameter.IsOptional ? ']' : '>').Append(' ');
            }

            return builder.Remove(builder.Length - 1, 1).ToString();
        }
    }
}