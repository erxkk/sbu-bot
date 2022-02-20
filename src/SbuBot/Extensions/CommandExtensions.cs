using System.ComponentModel;
using System.Linq;
using System.Text;

using Kkommon;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Commands.Parsing.HelperTypes;
using SbuBot.Commands.Parsing.TypeParsers;
using SbuBot.Models;

namespace SbuBot.Commands
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CommandExtensions
    {
        public static void AddTypeParserVariants<T>(this CommandService @this, TypeParser<T> normalParser)
        {
            @this.AddTypeParser(normalParser);
            @this.AddTypeParserVariants<T>();
        }

        public static void AddTypeParserVariants<T>(this CommandService @this)
            => @this.AddTypeParser(new OneOrAllTypeParser<T>());

        public static bool IsGroup(this Module @this) => @this.Name.EndsWith("Group");

        public static Command? GetDefaultCommand(this Module @this)
            => @this.Commands.FirstOrDefault(c => !c.Aliases.Any());

        public static void AppendTo(this Module @this, StringBuilder builder)
        {
            Preconditions.NotNull(@this, nameof(@this));
            Preconditions.NotNull(builder, nameof(builder));
            Preconditions.Greater(@this.FullAliases[0].Length, 0, "@this.FullAliases.Length");

            builder.Append(@this.FullAliases[0]);
        }

        public static string Format(this Module @this)
        {
            Preconditions.NotNull(@this, nameof(@this));
            Preconditions.Greater(@this.FullAliases[0].Length, 0, "@this.FullAliases.Length");

            StringBuilder builder = new(@this.FullAliases[0]);
            @this.AppendTo(builder);
            return builder.ToString();
        }

        public static void AppendTo(
            this Command @this,
            StringBuilder builder,
            bool withMeta = true,
            bool skipDescriptors = false
        )
        {
            Preconditions.NotNull(@this, nameof(@this));
            Preconditions.NotNull(builder, nameof(builder));
            Preconditions.Greater(@this.FullAliases[0].Length, 0, "@this.FullAliases.Length");

            builder.Append(@this.FullAliases[0]);

            if (@this.Parameters.Count == 0)
                return;

            builder.Append(' ');
            @this.AppendParametersTo(builder, withMeta, skipDescriptors);
        }

        public static string Format(this Command @this, bool withMeta = true, bool skipDescriptors = false)
        {
            Preconditions.NotNull(@this, nameof(@this));
            Preconditions.Greater(@this.FullAliases[0].Length, 0, "@this.FullAliases.Length");

            StringBuilder builder = new(@this.FullAliases[0].Length + 16 * @this.Parameters.Count);
            @this.AppendTo(builder, withMeta, skipDescriptors);
            return builder.ToString();
        }

        public static void AppendParametersTo(
            this Command @this,
            StringBuilder builder,
            bool withMeta = true,
            bool skipDescriptors = false
        )
        {
            Preconditions.NotNull(@this, nameof(@this));
            Preconditions.NotNull(builder, nameof(builder));

            foreach (Parameter parameter in @this.Parameters)
            {
                parameter.AppendTo(builder, withMeta, skipDescriptors);
                builder.Append(' ');
            }

            if (@this.Parameters.Count != 0)
                builder.Remove(builder.Length - 1, 1);
        }

        public static string FormatParameters(
            this Command @this,
            bool withMeta = true,
            bool skipDescriptors = false
        )
        {
            Preconditions.NotNull(@this, nameof(@this));

            StringBuilder builder = new(16 * @this.Parameters.Count);
            @this.AppendParametersTo(builder, withMeta, skipDescriptors);
            return builder.ToString();
        }

        public static void AppendTo(
            this Parameter @this,
            StringBuilder builder,
            bool withMeta = true,
            bool skipDescriptors = false
        )
        {
            Preconditions.NotNull(@this, nameof(@this));
            Preconditions.NotNull(builder, nameof(builder));

            builder.Append(@this.IsOptional ? '[' : '<');

            if (!skipDescriptors && @this.Type.IsAssignableTo(typeof(IDescriptor)))
            {
                // descriptors are always required and always the last lone non params arg
                builder.Append(string.Join("> :: <", IDescriptor.GetParts(@this.Type).Keys));
            }
            else
            {
                builder.Append(@this.Name);

                if (@this.IsMultiple)
                {
                    builder.Append(SbuGlobals.ELLIPSES);
                }
                else if (withMeta && @this.IsOptional)
                {
                    builder.Append(" = ");

                    object defaultValue = @this.Attributes
                        .OfType<OverrideDefaultAttribute>()
                        .FirstOrDefault() is { } overrideDefault
                        ? overrideDefault.Value
                        : @this.DefaultValue;

                    builder.Append(
                        defaultValue switch
                        {
                            null => "{none}",
                            true => "{true}",
                            false => "{false}",
                            string str when !str.StartsWith('{') => $"\"{str}\"",
                            { } value => value,
                        }
                    );

                    if (@this.Type.IsGenericType && @this.Type.GetGenericTypeDefinition() == typeof(OneOrAll<>))
                        builder.Append(" | all");
                }
                else if (withMeta
                         && @this.Type.IsGenericType
                         && @this.Type.GetGenericTypeDefinition() == typeof(OneOrAll<>))
                {
                    builder.Append(" | all");
                }
                else if (withMeta && @this.Type == typeof(SbuReminder))
                {
                    // create type appender?
                    builder.Append(" | last");
                }
            }

            builder.Append(@this.IsOptional ? ']' : '>');
        }

        public static string Format(this Parameter @this, bool withMeta = true, bool skipDescriptors = true)
        {
            Preconditions.NotNull(@this, nameof(@this));

            StringBuilder builder = new(2 + @this.Name.Length + (withMeta && @this.IsOptional ? 8 : 0));
            @this.AppendTo(builder, withMeta, skipDescriptors);
            return builder.ToString();
        }
    }
}
