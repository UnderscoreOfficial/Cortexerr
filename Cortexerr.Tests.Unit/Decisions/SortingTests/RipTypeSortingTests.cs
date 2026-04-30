using Cortexerr.Decisions.Logic.Sorting;

namespace Cortexerr.Tests.Unit.Decisions.SortingTests;

public class RipTypeSortingTests
{
    [Fact]
    public void Rip_Type_X_Has_Value()
    {
        var data = DecisionsData.Sort("webdl", "");
        var result = RipTypeSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Rip_Type_Y_Has_Value()
    {
        var data = DecisionsData.Sort("", "webdl");
        var result = RipTypeSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Rip_Type_X_Greater_Than()
    {
        var data = DecisionsData.Sort("remux", "webdl");
        var result = RipTypeSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Rip_Type_Y_Greater_Than()
    {
        var data = DecisionsData.Sort("webdl", "remux");
        var result = RipTypeSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Rip_Type_No_Values()
    {
        var data = DecisionsData.Sort("", "");
        var result = RipTypeSorting.Sort(data.x, data.y);
        Assert.Equal(0, result);
    }
}
