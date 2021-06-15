using System;

namespace SbuBot.Models
{
    public interface ISbuOwnedEntity : ISbuEntity
    {
        public Guid? OwnerId { get; set; }
        public SbuMember? Owner { get; }
    }
}