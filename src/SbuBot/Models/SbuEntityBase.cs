using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Destructurama.Attributed;

using SbuBot.Extensions;

namespace SbuBot.Models
{
    public abstract class SbuEntityBase
    {
        // generates yaml like representation
        public override string ToString()
        {
            var builder = new StringBuilder(512);
            builder.AppendLine($"{GetType().Name}:");

            appendMembers(ref builder, GetType().GetFields(BindingFlags.Instance | BindingFlags.Public));
            appendMembers(ref builder, GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public));

            return builder.ToString();

            void appendMembers(ref StringBuilder stringBuilder, IEnumerable<MemberInfo> members)
            {
                foreach (MemberInfo memberInfo in members)
                {
                    object? value = memberInfo switch
                    {
                        PropertyInfo { CanRead: true } propertyInfo => propertyInfo.GetValue(this),
                        FieldInfo fieldInfo => fieldInfo.GetValue(this),
                        _ => throw new ArgumentOutOfRangeException(nameof(members), memberInfo, null),
                    };

                    if (memberInfo.GetCustomAttribute<NotLoggedAttribute>() is { })
                        continue;

                    stringBuilder!.AppendLine(
                        (
                            $"{memberInfo.Name}: "
                            + value switch
                            {
                                // do not expand nested entities for now
                                SbuEntityBase sbuDbEntity => sbuDbEntity.GetType().Name,
                                string @string => $"\"{@string}\"",
                                IEnumerable enumerable =>
                                    "Enumerable<{"
                                    + (enumerable.GetType().GenericTypeArguments.FirstOrDefault()?.Name
                                        ?? "Unknown")
                                    + ">",
                                { } => value.ToString(),
                                null => "<null>",
                            }
                        )
                        .Indent(2)
                    );
                }
            }
        }
    }
}