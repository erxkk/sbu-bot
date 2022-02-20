using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Disqord;

using Qmmands;

using SbuBot.Commands.Attributes;
using SbuBot.Commands.Parsing.Descriptors;
using SbuBot.Commands.Parsing.HelperTypes;
using SbuBot.Models;

namespace SbuBot.Commands
{
    public static class Usage
    {
        public static readonly IReadOnlyDictionary<Type, IReadOnlyList<string>> TYPE_PARSING_EXAMPLES;
        public static readonly IReadOnlyList<string> NULLABLE_NOUNS = new[] { "null", "--" };
        public static readonly IReadOnlyList<string> UNKNOWN_USAGE = new[] { "???" };

        static Usage()
        {
            string[] members =
            {
                "user",
                "user#1234",
                "@user",
                "849571821930283018",
            };

            string[] roles =
            {
                "role",
                "@role",
                "849571821930283018",
            };

            string[] emojis =
            {
                "{emoji}",
                "849571821930283018",
            };

            string[] timestamps =
            {
                "tomorrow",
                "1h2m",
                "in 3 years",
            };

            // allow spaces for tags responses etc because they are always reminder or behind a descriptor
            Usage.TYPE_PARSING_EXAMPLES = new Dictionary<Type, IReadOnlyList<string>>
            {
                [typeof(string)] = new[] { "any non reserved text", "aaaa", "ooo" },
                [typeof(IMember)] = members,
                [typeof(SbuMember)] = members,
                [typeof(IRole)] = roles,
                [typeof(SbuRole)] = roles,
                [typeof(SbuColorRole)] = roles,
                [typeof(ICustomEmoji)] = emojis,
                [typeof(IGuildEmoji)] = emojis,
                [typeof(SbuReminder)] = new[] { "C9744FCE242603C", "849571821930283018" },
                [typeof(SbuTag)] = new[] { "tag name" },
                [typeof(SbuAutoResponse)] = new[] { "auto response trigger", "aaa", "ooo" },
                [typeof(TimeSpan)] = timestamps,
                [typeof(DateTime)] = timestamps,
                [typeof(DateTimeOffset)] = timestamps,
                [typeof(Guid)] = new[] { "8ceecbf3-1315-4c20-af06-3b3f8a570460" },
                [typeof(Color)] = new[] { "green", "#afafaf" },
                [typeof(Uri)] = new[]
                {
                    "https://discord.com/channels/732210852849123418/732231139233759324/836993360274784297",
                },
            };
        }

        public static List<string> GetUsages(Type type)
        {
            if (type.IsAssignableTo(typeof(Enum)))
            {
                string[] names = Enum.GetNames(type);
                return names.ToList();
            }

            // TODO: randomize which descriptor sub type usage is returned here + return more then one
            if (type.IsAssignableTo(typeof(IDescriptor)))
            {
                IReadOnlyDictionary<string, Type> parts = IDescriptor.GetParts(type);

                // throw if non existent
                IEnumerable<string> typeUsages = parts.Select(t => Usage.TYPE_PARSING_EXAMPLES[t.Value][0]);
                return Enumerable.Repeat(string.Join(" :: ", typeUsages), 1).ToList();
            }

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return (Usage.TYPE_PARSING_EXAMPLES.GetValueOrDefault(type.GenericTypeArguments[0])
                            ?? Usage.UNKNOWN_USAGE)
                        .Concat(Usage.NULLABLE_NOUNS)
                        .ToList();

                if (type.GetGenericTypeDefinition() == typeof(OneOrAll<>))
                    return (Usage.TYPE_PARSING_EXAMPLES.GetValueOrDefault(type.GenericTypeArguments[0])
                            ?? Usage.UNKNOWN_USAGE)
                        .Append("all")
                        .ToList();

                throw new ArgumentOutOfRangeException();
            }

            return (Usage.TYPE_PARSING_EXAMPLES.GetValueOrDefault(type) ?? Usage.UNKNOWN_USAGE).ToList();
        }

        public static IEnumerable<string> GetUsages(Command command)
        {
            if (command.Attributes.OfType<UsageOverrideAttribute>().FirstOrDefault() is { } usage)
                return usage.Values;

            if (command.Parameters.Count == 0)
                return new[] { command.FullAliases[0] };

            // we want to get at least as many usages as the parameter usage max in this example `a`
            // and for all parameters with less we just fill out the last known usage
            // List<List<string>> [
            //     ["a1", "a2", "a3"],
            //     ["b1", "b2"],
            //     ["c1"],
            //     ["d1", "d2"]
            // ]
            // List<string> [
            //     "a1 b1 c1 d1",
            //     "a2 b2 c1 d2",
            //     "a3 b2 c1 d2",
            // ]

            // TODO: alternate instead of dragging the lat one for less than max usages
            // List<string> [
            //     "a1 b1 c1 d1",
            //     "a2 b2 c1 d2",
            //     "a3 b1 c1 d1",
            // ]

            List<List<string>> parameterUsages = command.Parameters
                .Select(p => Usage.GetUsages(p.Type))
                .ToList();

            int maxUsages = parameterUsages.Max(u => u.Count);
            List<string> usages = new(maxUsages);

            StringBuilder buffer = new(
                command.FullAliases[0],
                command.FullAliases[0].Length + command.Parameters.Count * 64
            );

            for (int u = 0; u < maxUsages; u++)
            {
                for (int p = 0; p < command.Parameters.Count; p++)
                    buffer.Append(' ').Append(parameterUsages[p][Math.Min(u, parameterUsages[p].Count - 1)]);

                usages.Add(buffer.ToString());
                buffer.Remove(command.FullAliases[0].Length, buffer.Length - command.FullAliases[0].Length);
            }

            return usages;
        }

        public static IEnumerable<string> GetUsages(Module module)
        {
            if (module.Attributes.OfType<UsageOverrideAttribute>().FirstOrDefault() is { } usage)
                return usage.Values;

            return module.GetDefaultCommand() is { }
                ? new[] { $"{module.FullAliases[0]} {SbuGlobals.ELLIPSES}" }
                : new[]
                {
                    $"{module.FullAliases[0]} {SbuGlobals.ELLIPSES}",
                    $"{module.FullAliases[0]} {{subCommand}} {SbuGlobals.ELLIPSES}",
                };
        }
    }
}
