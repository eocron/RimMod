using System.Collections.Generic;
using System.Linq;

namespace RimMod.Synchronization
{
    public sealed class ComplexRawIdParser : IRawIdParser
    {
        private readonly IRawIdParser[] _all;

        public ComplexRawIdParser(IRawIdParser[] all)
        {
            _all = all;
        }

        public IEnumerable<IItemId> Parse(string text)
        {
            return _all
                .SelectMany(x => x.Parse(text))
                .Distinct();
        }
    }
}