using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Aethria.Application.Utils;

internal static class PdfParsingUtils
{
    public static string ExtractTextFromPdf(Stream fileStream)
    {
        var pageTexts = new List<string>();

        using (var document = PdfDocument.Open(fileStream))
        {
            foreach (var page in document.GetPages())
            {
                var text = ContentOrderTextExtractor.GetText(page, true);
                pageTexts.Add(text);
            }
        }

        var headerFooterPatterns = DetectRepeatingHeaderFooter(pageTexts);
        var cleanedPages = RemoveHeaderFooter(pageTexts, headerFooterPatterns);

        var content = string.Join("\n\n", cleanedPages);
        content = content.Replace("\r\n", "\n");

        content = Regex.Replace(content, @"(?m)^\s*(Page|Trang)\s+\d+(\s*(of|/)\s*\d+)?\s*$", "", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"(?m)^\s*-?\s*\d+\s*-?\s*$", "");
        content = Regex.Replace(content, @"(?m)^\s*\d+\s*/\s*\d+\s*$", "");

        content = Regex.Replace(content, @"\n{3,}", "\n\n");

        content = Regex.Replace(content,
            @"([^\.\!\?\:\n])\s*\n\s*([a-zàáảãạăắằẳẵặâấầẩẫậèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵđ])",
            "$1 $2");

        content = Regex.Replace(content, @"-\s*\n\s*", "");

        return content;
    }

    private static List<Regex> DetectRepeatingHeaderFooter(List<string> pageTexts)
    {
        if (pageTexts.Count < 3)
            return [];

        var candidatePatterns = new Dictionary<string, int>();

        foreach (var pageText in pageTexts)
        {
            var lines = pageText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) continue;

            var candidates = new List<string>();
            candidates.AddRange(lines.Take(2));
            candidates.AddRange(lines.TakeLast(2));

            foreach (var line in candidates)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length < 5)
                    continue;

                var normalized = Regex.Replace(trimmed, @"\s*\d+\s*$", " {NUM}");
                normalized = Regex.Replace(normalized, @"^\s*\d+\s*", "{NUM} ");

                if (!candidatePatterns.ContainsKey(normalized))
                    candidatePatterns[normalized] = 0;
                candidatePatterns[normalized]++;
            }
        }

        var threshold = pageTexts.Count / 2;
        var patterns = new List<Regex>();

        foreach (var kvp in candidatePatterns)
        {
            if (kvp.Value >= threshold)
            {
                var escaped = Regex.Escape(kvp.Key)
                    .Replace(@"\{NUM\}", @"\s*\d+\s*");
                patterns.Add(new Regex(@"^\s*" + escaped + @"\s*$", RegexOptions.IgnoreCase));
            }
        }

        return patterns;
    }

    private static List<string> RemoveHeaderFooter(List<string> pageTexts, List<Regex> patterns)
    {
        if (patterns.Count == 0)
            return pageTexts;

        var result = new List<string>();

        foreach (var pageText in pageTexts)
        {
            var lines = pageText.Split('\n');
            var filtered = lines.Where(line => !patterns.Any(p => p.IsMatch(line))).ToArray();
            result.Add(string.Join("\n", filtered));
        }

        return result;
    }
}
