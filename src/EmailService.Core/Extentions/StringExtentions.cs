namespace EmailService.Core;

public static class StringExtentions
{
    public static bool IsBase64String(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        var buffer = new Span<byte>(new byte[str.Length]);
        return Convert.TryFromBase64String(str, buffer, out int bytesParsed);
    }
}
