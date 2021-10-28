using System;

namespace SbuBot.Commands.Attributes
{
    // TODO: remove or implement command mapping
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class CommandMappingAttribute : Attribute
    {
        public string Aliased { get; }
        public string Alias { get; }

        public CommandMappingAttribute(string aliased, string alias)
        {
            Aliased = aliased;
            Alias = alias;
        }
    }
}