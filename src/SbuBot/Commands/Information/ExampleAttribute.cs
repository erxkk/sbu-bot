using System;

namespace SbuBot.Commands.Information
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class ExampleAttribute : Attribute
    {
        public string Value { get; }

        public ExampleAttribute(string value) => Value = value;
    }
}