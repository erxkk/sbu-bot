using System;

namespace SbuBot.Models
{
    [Flags]
    public enum SbuGuildConfig : byte
    {
        Respond,
        Archive,
    }
}