using System;

namespace SbuBot.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class HideOnSerializeAttribute : Attribute { }

    public sealed class HiddenValue
    {
        public static readonly HiddenValue INSTANCE = new();
    }
}