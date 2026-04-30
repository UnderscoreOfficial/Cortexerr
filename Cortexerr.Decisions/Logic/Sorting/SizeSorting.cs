namespace Cortexerr.Decisions.Logic.Sorting;

public static class SizeSorting
{
    public static int Sort(NormalizedSizes normalized_sizes)
    {
        var x_size = normalized_sizes.x_size;
        var y_size = normalized_sizes.y_size;
        var x_size_multiplier = normalized_sizes.x_size_multiplier;
        var y_size_multiplier = normalized_sizes.y_size_multiplier;

        if (x_size > y_size * y_size_multiplier)
        {
            return -1;
        }
        if (y_size > x_size * x_size_multiplier)
        {
            return 1;
        }
        return 0;
    }
}
