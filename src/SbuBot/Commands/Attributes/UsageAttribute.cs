using System;

namespace SbuBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class UsageAttribute : Attribute
    {
        public string[] Values { get; }

        public UsageAttribute(params string[] values) => Values = values;
    }
}