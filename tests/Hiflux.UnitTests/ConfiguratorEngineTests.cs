using Hiflux.Infrastructure.Data.Configurator;

namespace Hiflux.UnitTests;

public class ConfiguratorEngineTests
{
    // Two filters; three products:
    //   100: Size 1/2", End NPT
    //   101: Size 3/4", End NPT
    //   102: Size 1/2", End BSP
    private static readonly List<FilterDef> Filters =
    [
        new FilterDef(1, "Size", 0),
        new FilterDef(2, "End Connection", 1),
    ];

    private static List<SpecRow> Rows() =>
    [
        new SpecRow(100, "Size", "1/2\""), new SpecRow(100, "End Connection", "NPT"),
        new SpecRow(101, "Size", "3/4\""), new SpecRow(101, "End Connection", "NPT"),
        new SpecRow(102, "Size", "1/2\""), new SpecRow(102, "End Connection", "BSP"),
    ];

    [Fact]
    public void Empty_selection_matches_everything_and_all_values_are_available()
    {
        var result = ConfiguratorEngine.Compute(Filters, Rows(), new Dictionary<string, string>());

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.MatchCount);
        Assert.Null(result.ResolvedProductId);
        Assert.All(result.Options, o => Assert.All(o.Values, v => Assert.True(v.Available)));
    }

    [Fact]
    public void Selecting_a_value_greys_out_unreachable_values_of_other_filters()
    {
        // Size 3/4" exists only on product 101, whose End is NPT -> BSP becomes unreachable.
        var selection = new Dictionary<string, string> { ["Size"] = "3/4\"" };

        var result = ConfiguratorEngine.Compute(Filters, Rows(), selection);

        var endConnection = result.Options.Single(o => o.Name == "End Connection");
        Assert.True(endConnection.Values.Single(v => v.Value == "NPT").Available);
        Assert.False(endConnection.Values.Single(v => v.Value == "BSP").Available);
    }

    [Fact]
    public void Selection_that_narrows_to_one_product_resolves_it()
    {
        var selection = new Dictionary<string, string> { ["Size"] = "3/4\"" };

        var result = ConfiguratorEngine.Compute(Filters, Rows(), selection);

        Assert.Equal(1, result.MatchCount);
        Assert.Equal(101, result.ResolvedProductId);
    }

    [Fact]
    public void Selection_matching_multiple_products_does_not_resolve()
    {
        var selection = new Dictionary<string, string> { ["Size"] = "1/2\"" }; // products 100 and 102

        var result = ConfiguratorEngine.Compute(Filters, Rows(), selection);

        Assert.Equal(2, result.MatchCount);
        Assert.Null(result.ResolvedProductId);
    }

    [Fact]
    public void Value_matching_is_case_insensitive()
    {
        var selection = new Dictionary<string, string> { ["End Connection"] = "npt" }; // lowercase

        var result = ConfiguratorEngine.Compute(Filters, Rows(), selection);

        Assert.Equal(2, result.MatchCount); // products 100 and 101 (both NPT)
    }
}
