using Cortexerr.Decisions.Logic.Sorting;

namespace Cortexerr.Tests.Unit.Decisions.SortingTests;

public class ResolutionSortingTests
{
    [Fact]
    public void Resolution_X_Has_Value()
    {
        var data = DecisionsData.Sort("1080p", "");
        var result = ResolutionSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Resolution_Y_Has_Value()
    {
        var data = DecisionsData.Sort("", "1080p");
        var result = ResolutionSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Resolution_X_Greater_Than()
    {
        var data = DecisionsData.Sort("2160p", "1080p");
        var result = ResolutionSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Resolution_Y_Greater_Than()
    {
        var data = DecisionsData.Sort("1080p", "2160p");
        var result = ResolutionSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Resolution_No_Values()
    {
        var data = DecisionsData.Sort("", "");
        var result = DolbyVisionSorting.Sort(data.x, data.y);
        Assert.Equal(0, result);
    }
}
