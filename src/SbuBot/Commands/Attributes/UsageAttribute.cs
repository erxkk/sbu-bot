using System;

namespace SbuBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UsageAttribute : Attribute
    {
        public string[] Values { get; }

        public UsageAttribute(params string[] values) => Values = values;
    }
}