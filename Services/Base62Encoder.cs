namespace UrlShortener.Api.Services;

/// <summary>
/// Encodes non-negative integers as Base62 strings (0-9, A-Z, a-z) and back.
/// Used to turn an auto-increment row Id into a short, URL-safe, collision-free code
/// without relying on randomness or GUIDs.
/// </summary>
public static class Base62Encoder
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int Base = 62;

    public static string Encode(long value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be non-negative.");

        if (value == 0)
            return Alphabet[0].ToString();

        // long.MaxValue needs at most 11 base62 digits.
        Span<char> buffer = stackalloc char[11];
        var index = buffer.Length;

        while (value > 0)
        {
            var remainder = (int)(value % Base);
            buffer[--index] = Alphabet[remainder];
            value /= Base;
        }

        return new string(buffer[index..]);
    }

    public static long Decode(string code)
    {
        ArgumentException.ThrowIfNullOrEmpty(code);

        long result = 0;
        foreach (var c in code)
        {
            var digit = Alphabet.IndexOf(c);
            if (digit < 0)
                throw new FormatException($"'{c}' is not a valid Base62 character.");

            result = (result * Base) + digit;
        }

        return result;
    }
}
