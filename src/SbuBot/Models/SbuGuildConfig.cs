using System;

namespace SbuBot.Models
{
    [Flags]
    public enum SbuGuildConfig : byte
    {
        None = 0,
        Respond = 1 << 0,
        Archive = 1 << 1,
        All = 255,
    }
}
