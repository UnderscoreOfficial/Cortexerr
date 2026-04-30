using Cortexerr.Decisions.Logic.Sorting;

namespace Cortexerr.Tests.Unit.Decisions.SortingTests;

public class HighDynamicRangeSortingTests
{
    [Fact]
    public void High_Dynamic_Range_X_Has_Value()
    {
        var data = DecisionsData.Sort("hdr", "");
        var result = HighDynamicRangeSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void High_Dynamic_Range_Y_Has_Value()
    {
        var data = DecisionsData.Sort("", "hdr");
        var result = HighDynamicRangeSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void High_Dynamic_Range_No_Values()
    {
        var data = DecisionsData.Sort("", "");
        var result = HighDynamicRangeSorting.Sort(data.x, data.y);
        Assert.Equal(0, result);
    }
}
