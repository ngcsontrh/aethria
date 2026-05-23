using System.Text;
using System.Text.RegularExpressions;

namespace Aethria.Application.Utils;

internal static class TxtParsingUtils
{
    private static readonly Encoding[] SupportedEncodings =
    [
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true),
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
        Encoding.Unicode,        // UTF-16 LE
        Encoding.BigEndianUnicode, // UTF-16 BE
        Encoding.UTF32,
        Encoding.GetEncoding("windows-1252"), // Common Western European fallback
        Encoding.Latin1
    ];

    /// <summary>
    /// Extracts and cleans text content from a plain text (.txt) file stream.
    /// Handles BOM detection and encoding auto-detection.
    /// </summary>
    public static string ExtractTextFromTxt(Stream fileStream)
    {
        var content = ReadStreamWithEncodingDetection(fileStream);

        content = content.Replace("\r\n", "\n").Replace("\r", "\n");

        content = TextSanitizationUtils.RemoveNullCharacters(content);

        content = Regex.Replace(content, @"\n{3,}", "\n\n");

        content = Regex.Replace(content, @"[ \t]+\n", "\n");

        content = content.Trim();

        return content;
    }

    private static string ReadStreamWithEncodingDetection(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();

        if (bytes.Length == 0)
            return string.Empty;

        var encoding = DetectEncodingFromBom(bytes);
        if (encoding != null)
        {
            var preamble = encoding.GetPreamble();
            return encoding.GetString(bytes, preamble.Length, bytes.Length - preamble.Length);
        }

        foreach (var candidate in SupportedEncodings)
        {
            try
            {
                var decoder = candidate.GetDecoder();
                var charCount = decoder.GetCharCount(bytes, 0, bytes.Length, flush: true);
                var chars = new char[charCount];
                decoder.GetChars(bytes, 0, bytes.Length, chars, 0, flush: true);
                return new string(chars);
            }
            catch (DecoderFallbackException)
            {
            }
        }

        return Encoding.Latin1.GetString(bytes);
    }

    private static Encoding? DetectEncodingFromBom(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8;

        if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            return Encoding.UTF32; // UTF-32 BE — check before UTF-16

        if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            return Encoding.UTF32;

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode; // UTF-16 LE

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode; // UTF-16 BE

        return null;
    }
}
