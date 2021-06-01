using System;

namespace SbuBot.Commands.Information
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class OverrideDefaultAttribute : Attribute
    {
        public string Value { get; }

        public OverrideDefaultAttribute(string value) => Value = value;
    }
}