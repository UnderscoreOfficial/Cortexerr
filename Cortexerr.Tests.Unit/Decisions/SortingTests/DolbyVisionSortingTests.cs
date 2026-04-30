using Cortexerr.Decisions.Logic.Sorting;

namespace Cortexerr.Tests.Unit.Decisions.SortingTests;

public class DolbyVisionSortingTests
{
    [Fact]
    public void Dolby_Vision_X_Has_Value()
    {
        var data = DecisionsData.Sort("dolby vision", "");
        var result = DolbyVisionSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Dolby_Vision_Y_Has_Value()
    {
        var data = DecisionsData.Sort("", "dolby vision");
        var result = DolbyVisionSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Dolby_Vision_No_Values()
    {
        var data = DecisionsData.Sort("", "");
        var result = DolbyVisionSorting.Sort(data.x, data.y);
        Assert.Equal(0, result);
    }
}
