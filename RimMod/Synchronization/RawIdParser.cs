using System.Collections.Generic;
using System.Linq;

namespace RimMod.Synchronization
{
    public sealed class RawIdParser<T> : IRawIdParser
    {
        private readonly IIdParser<T> _inner;

        public RawIdParser(IIdParser<T> inner)
        {
            _inner = inner;
        }

        public IEnumerable<IItemId> Parse(string text)
        {
            return _inner.Parse(text).Select(x => (IItemId)x);
        }
    }
}