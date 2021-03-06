using System;

namespace SbuBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class OverrideDefaultAttribute : Attribute
    {
        public object Value { get; }

        public OverrideDefaultAttribute(object value) => Value = value;
    }
}
