using System.Security.Cryptography;

namespace Cortexerr.Core.Utilities;

/// <summary>
/// Reusable utility methods
/// </summary>
public static class Utils
{

    public static string SecureRandomHexadecimal(int bytes = 32)
    {
        var buffer = new byte[bytes];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer).ToLowerInvariant();
    }

    public static string RandomHexadecimal(int length)
    {
        const string hex_characters = "0123456789ABCDEF";
        var random_hex = "";
        var valid_length = length;
        if (length < 1) valid_length = 1;
        for (var i = 0; i < valid_length; i++)
        {
            random_hex += hex_characters[Random.Shared.Next(16)];
        }
        return random_hex;
    }

    public static long RandomByteSize(double max_gigabyte, double min_gigabyte = 0.5)
    {
        const double BYTE_MULTIPLIER = 1_000_000_000d;

        if (!double.IsFinite(min_gigabyte)) min_gigabyte = 0.5;
        if (!double.IsFinite(max_gigabyte)) max_gigabyte = 50.0;

        long min_bytes = (long)(min_gigabyte * BYTE_MULTIPLIER);
        long max_bytes = (long)(max_gigabyte * BYTE_MULTIPLIER);

        if (max_bytes < min_bytes)
            max_bytes = min_bytes;
        max_bytes = max_bytes == long.MaxValue ? max_bytes : max_bytes + 1;

        return Random.Shared.NextInt64(min_bytes, max_bytes);
    }

    public static DateTime RandomDateTime(int max_years = 3)
    {
        var end = DateTime.UtcNow;
        var start = new DateTime(end.Year - max_years, 1, 1);
        var random_date = start.AddMinutes(Random.Shared.Next((end - start).Days * 1440));
        return random_date;
    }
}
