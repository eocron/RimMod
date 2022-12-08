using System;

namespace RimMod.Synchronization
{
    public interface IRawItemProvider : IItemProvider<IItemId, IItem>
    {
        Type ItemIdType { get; }
    }
}