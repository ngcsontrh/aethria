namespace Aethria.Application.Utils;

internal static class TextSanitizationUtils
{
    public static string RemoveNullCharacters(string value)
    {
        return value.Replace("\0", string.Empty);
    }

    public static string? RemoveNullCharactersOrNull(string? value)
    {
        return value is null ? null : RemoveNullCharacters(value);
    }
}
