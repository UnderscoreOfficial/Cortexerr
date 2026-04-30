using Cortexerr.Decisions.Logic.Sorting;

namespace Cortexerr.Tests.Unit.Decisions.SortingTests;

public class AudioCodecSortingTests
{
    [Fact]
    public void Audio_Codec_X_Has_Value()
    {
        var data = DecisionsData.Sort("atmos", "");
        var result = AudioCodecSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Audio_Codec_Y_Has_Value()
    {
        var data = DecisionsData.Sort("", "atmos");
        var result = AudioCodecSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Audio_Codec_X_Greater_Than()
    {
        var data = DecisionsData.Sort("atmos", "dts");
        var result = AudioCodecSorting.Sort(data.x, data.y);
        Assert.Equal(-1, result);
    }
    [Fact]
    public void Audio_Codec_Y_Greater_Than()
    {
        var data = DecisionsData.Sort("dts", "atmos");
        var result = AudioCodecSorting.Sort(data.x, data.y);
        Assert.Equal(1, result);
    }
    [Fact]
    public void Audio_Codec_No_Values()
    {
        var data = DecisionsData.Sort("", "");
        var result = DolbyVisionSorting.Sort(data.x, data.y);
        Assert.Equal(0, result);
    }
}
