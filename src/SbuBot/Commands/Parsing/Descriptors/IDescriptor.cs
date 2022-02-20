using System;
using System.Collections.Generic;

namespace SbuBot.Commands.Parsing.Descriptors
{
    public interface IDescriptor
    {
        public static string GetRemarks(Type type)
        {
            if (!type.IsAssignableTo(typeof(IDescriptor)))
                throw new ArgumentOutOfRangeException(nameof(type), "Argument is not an IDescriptor");

            return (type.GetField("REMARKS")!.GetValue(null) as string)!;
        }

        public static IReadOnlyDictionary<string, Type> GetParts(Type type)
        {
            if (!type.IsAssignableTo(typeof(IDescriptor)))
                throw new ArgumentOutOfRangeException(nameof(type), "Argument is not an IDescriptor");

            return (type.GetField("PARTS")!.GetValue(null) as IReadOnlyDictionary<string, Type>)!;
        }
    }
}
