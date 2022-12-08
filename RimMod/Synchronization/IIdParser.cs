using System.Collections.Generic;

namespace RimMod.Synchronization
{
    public interface IIdParser<out TId>
    {
        IEnumerable<TId> Parse(string text);
    }
}