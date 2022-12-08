using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RimMod.Synchronization
{
    public sealed class RawItemProvider<TId, TItem> : IRawItemProvider
    {
        private readonly IItemProvider<TId, TItem> _inner;

        public RawItemProvider(IItemProvider<TId, TItem> inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public async Task<IList<IItem>> GetItemsAsync(ICollection<IItemId> itemIds, CancellationToken cancellationToken)
        {
            var result = await _inner.GetItemsAsync(itemIds.Select(x => (TId)x).ToList(), cancellationToken)
                .ConfigureAwait(false);
            return result.Select(x => (IItem)x).ToList();
        }

        public async Task<IList<IItem>> GetAllItemsAsync(CancellationToken cancellationToken)
        {
            var result = await _inner.GetAllItemsAsync(cancellationToken)
                .ConfigureAwait(false);
            return result.Select(x => (IItem)x).ToList();
        }

        public Type ItemIdType => typeof(TId);
    }
}