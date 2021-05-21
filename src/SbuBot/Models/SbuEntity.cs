using System;

namespace SbuBot.Models
{
    public abstract record SbuEntity
    {
        public Guid Id { get; }

        // new
        protected SbuEntity() => Id = Guid.NewGuid();

        // ef core
        internal SbuEntity(Guid id) => Id = id;
    }
}