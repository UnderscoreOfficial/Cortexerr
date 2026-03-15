using Cortexerr.Core.Utilities;

namespace Cortexerr.Tests.Unit.Core;

public class UtilsTest
{
    public const double BYTE_MULTIPLE = 1000000000.00;
    [Fact]
    public void Random_Hex_Invalid_Length()
    {
        var result = Utils.RandomHexadecimal(-5);
        Assert.Equal(1, result.Length);
    }
    [Fact]
    public void Random_Hex_Valid_Length()
    {
        var result = Utils.RandomHexadecimal(5);
        Assert.Equal(5, result.Length);
    }
    [Fact]
    public void Random_Byte_Size_Finite_Max_Gigabyte()
    {
        var result = Utils.RandomByteSize(double.NaN, 10);
        Assert.True((50 * BYTE_MULTIPLE) >= result);
        Assert.True((10 * BYTE_MULTIPLE) <= result);
    }
    [Fact]
    public void Random_Byte_Size_Finite_Min_Gigabyte()
    {
        var result = Utils.RandomByteSize(10, double.NaN);
        Assert.True((10 * BYTE_MULTIPLE) >= result);
        Assert.True((0.5 * BYTE_MULTIPLE) <= result);
    }
    [Fact]
    public void Random_Byte_Size_Min_Value_Clamping()
    {
        var result = Utils.RandomByteSize(1, 5);
        Assert.Equal(5 * BYTE_MULTIPLE, result);
    }
    [Fact]
    public void Random_Byte_Size_Valid_Input()
    {
        var result = Utils.RandomByteSize(50, 5);
        Assert.True(50 * BYTE_MULTIPLE >= result);
        Assert.True(5 * BYTE_MULTIPLE <= result);
    }
}
