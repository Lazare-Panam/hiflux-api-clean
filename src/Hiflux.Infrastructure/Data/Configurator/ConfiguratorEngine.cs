using Hiflux.Core.Dtos;

namespace Hiflux.Infrastructure.Data.Configurator;

public record FilterDef(int FilterId, string Name, int SortOrder);
public record SpecRow(int ProductId, string FilterName, string Value);
public record ConfiguratorResult(List<ConfiguratorOptionDto> Options, int MatchCount, int TotalCount, int? ResolvedProductId);
public static class ConfiguratorEngine
{
    public static ConfiguratorResult Compute(IReadOnlyList<FilterDef> filterDefs, IReadOnlyList<SpecRow> rows, IReadOnlyDictionary<string, string> selection)
    {
        var productSpecs = new Dictionary<int, Dictionary<string, string>>();
        foreach (var row in rows)
        {
            if (!productSpecs.TryGetValue(row.ProductId, out var specs))
            {
                specs = new Dictionary<string, string>();
                productSpecs[row.ProductId] = specs;
            }
            specs[row.FilterName] = row.Value;
        }

        var allIds = productSpecs.Keys.ToList();

        bool MatchesSelection(Dictionary<string, string> specs, string? ignoreFilter)
        {
            foreach (var (filterName, pickedValue) in selection)
            {
                if (filterName == ignoreFilter)
                    continue;
                if (!specs.TryGetValue(filterName, out var productValue))
                    return false;
                if (!string.Equals(productValue, pickedValue, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

        var matchIds = allIds.Where(id => MatchesSelection(productSpecs[id], null)).ToList();

        var options = new List<ConfiguratorOptionDto>();
        foreach (var fd in filterDefs)
        {
            var allValues = rows
                .Where(r => r.FilterName == fd.Name)
                .Select(r => r.Value)
                .Distinct()
                .ToList();

            var reachable = allIds.Where(id => MatchesSelection(productSpecs[id], fd.Name)).ToHashSet();
            var availableValues = rows
                .Where(r => r.FilterName == fd.Name && reachable.Contains(r.ProductId))
                .Select(r => r.Value)
                .ToHashSet();

            options.Add(new ConfiguratorOptionDto
            {
                FilterId = fd.FilterId,
                Name = fd.Name,
                SortOrder = fd.SortOrder,
                Values = allValues
                    .Select(v => new ConfiguratorValueDto { Value = v, Available = availableValues.Contains(v) })
                    .ToList(),
            });
        }

        int? resolvedProductId = matchIds.Count == 1 ? matchIds[0] : null;

        return new ConfiguratorResult(options, matchIds.Count, allIds.Count, resolvedProductId);
    }
}
