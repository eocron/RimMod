using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RimMod.Synchronization;

public sealed class ComplexItemProvider : IItemProvider<IItemId, IItem>
{
    private readonly Dictionary<Type, IRawItemProvider> _all;

    public ComplexItemProvider(IRawItemProvider[] all)
    {
        _all = all.ToDictionary(x=> x.ItemIdType);
    }
    public async Task<IList<IItem>> GetItemsAsync(ICollection<IItemId> itemIds, CancellationToken cancellationToken)
    {
        var result = await Task.WhenAll(itemIds.DistinctBy(x=> x).GroupBy(x => x.GetType())
                .Select(x => _all[x.Key].GetItemsAsync(x.ToList(), cancellationToken)))
            .ConfigureAwait(false);
        return result.SelectMany(x => x).DistinctBy(x => x.Id).ToList();
    }

    public async Task<IList<IItem>> GetAllItemsAsync(CancellationToken cancellationToken)
    {
        var result = await Task.WhenAll(_all.Select(x => x.Value.GetAllItemsAsync(cancellationToken)))
            .ConfigureAwait(false);
        return result.SelectMany(x => x).DistinctBy(x=> x.Id).ToList();
    }
}