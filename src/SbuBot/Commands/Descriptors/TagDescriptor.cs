namespace SbuBot.Commands.Descriptors
{
    public readonly struct TagDescriptor : IDescriptor
    {
        public string Name { get; init; }
        public string Content { get; init; }
    }
}