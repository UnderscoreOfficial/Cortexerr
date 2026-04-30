using Cortexerr.Decisions.Logic.Sorting;

namespace Cortexerr.Tests.Unit.Decisions.SortingTests;

public class SizeSortingTests
{
    [Fact]
    public void Size_X_Greater_Than()
    {
        var data = DecisionsData.Sort("", "", 1000000, 1000);
        var result = SizeSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Size_Y_Greater_Than()
    {
        var data = DecisionsData.Sort("", "", 1000, 1000000);
        var result = SizeSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Size_X_Greater_Than_Tie_Breaker()
    {
        var data = DecisionsData.Sort("x264 hdr", "x264", 1000000, 1000001);
        var result = SizeSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Size_Y_Greater_Than_Tie_Breaker()
    {
        var data = DecisionsData.Sort("x264", "x264 hdr", 1000001, 1000000);
        var result = SizeSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Tie_Breaker_With_No_Tie_Breaks_Size_FallBack()
    {
        var data = DecisionsData.Sort("x264", "x264", 1000001, 1000000);
        var result = SizeSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Tie_Breaker_With_No_Tie_Breaks_Same_Size()
    {
        var data = DecisionsData.Sort("x264", "x264", 1000000, 1000000);
        var result = SizeSorting.Sort(data.x, data.y);
        Assert.Equal(0, result);
    }
    [Fact]
    public void Size_No_Values()
    {
        var data = DecisionsData.Sort("", "", 0, 0);
        var result = SizeSorting.Sort(data.x, data.y);
        Assert.Equal(0, result);
    }
}
