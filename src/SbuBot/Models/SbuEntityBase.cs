using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using SbuBot.Extensions;

namespace SbuBot.Models
{
    public abstract class SbuEntityBase : ISbuEntity
    {
        public Guid Id { get; }

        // new
        protected SbuEntityBase() => Id = Guid.NewGuid();

        // ef core
        internal SbuEntityBase(Guid id) => Id = id;

        // generates yaml like representation
        public override string ToString()
        {
            var builder = new StringBuilder(512);
            builder.AppendLine($"{GetType().Name}:");

            appendMembers(ref builder, GetType().GetFields());
            appendMembers(ref builder, GetType().GetProperties());

            return builder.ToString();

            void appendMembers(ref StringBuilder stringBuilder, IEnumerable<MemberInfo> members)
            {
                foreach (MemberInfo memberInfo in members)
                {
                    object? value = memberInfo switch
                    {
                        PropertyInfo { CanRead: true } propertyInfo => propertyInfo.GetValue(this),
                        FieldInfo fieldInfo => fieldInfo.GetValue(this),
                        _ => throw new ArgumentOutOfRangeException(nameof(members), memberInfo, null)
                    };

                    value = memberInfo.GetCustomAttribute<HideOnSerializeAttribute>() is { }
                        ? HiddenValue.INSTANCE
                        : value;

                    if (value is HiddenValue)
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
                                    + (enumerable.GetType().GenericTypeArguments.FirstOrDefault()?.Name ?? "Unknown")
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