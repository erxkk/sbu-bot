using System;

namespace SbuBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public sealed class UsageOverrideAttribute : Attribute
    {
        public string[] Values { get; }

        public UsageOverrideAttribute(params string[] values) => Values = values;
    }
}
