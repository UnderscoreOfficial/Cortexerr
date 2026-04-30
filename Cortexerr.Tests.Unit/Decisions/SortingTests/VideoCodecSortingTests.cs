using Cortexerr.Decisions.Logic.Sorting;

namespace Cortexerr.Tests.Unit.Decisions.SortingTests;

public class VideoCodecSortingTests
{
    [Fact]
    public void Video_Codec_X_Has_Value()
    {
        var data = DecisionsData.Sort("x264", "");
        var result = VideoCodecSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Video_Codec_Y_Has_Value()
    {
        var data = DecisionsData.Sort("", "x264");
        var result = VideoCodecSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Video_Codec_X_Greater_Than()
    {
        var data = DecisionsData.Sort("x265", "x264");
        var result = VideoCodecSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Video_Codec_Y_Greater_Than()
    {
        var data = DecisionsData.Sort("x264", "x265");
        var result = VideoCodecSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Video_Codec_No_Values()
    {
        var data = DecisionsData.Sort("", "");
        var result = VideoCodecSorting.Sort(data.x, data.y);
        Assert.Equal(0, result);
    }
}
