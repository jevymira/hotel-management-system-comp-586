using System.Text;

namespace Domain.Services;

public static class IdGenerator
{
    private static Random _random = new Random();
    private static char[] _base62Chars =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
        .ToCharArray();

    // 62^6 = 56+ billion unique IDs
    // for readability, acceptable for long-term storage at small scale;
    // otherwise, would consider Guid
    public static string Get6CharBase62()
    {
        var sb = new StringBuilder(6);
        for (int i = 0; i < 6; i++)
            sb.Append(_base62Chars[_random.Next(62)]);
        return sb.ToString();
    }

    // 10^10 = 10 billion unique IDs
    // to enforce uniqueness, check for duplicate(s) at creation
    public static string Get10CharNumericBase10()
    {
        return _random.Next(100000000, 2147483647).ToString().PadLeft(10, '0');
    }
}
