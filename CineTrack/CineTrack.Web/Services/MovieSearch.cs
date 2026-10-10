using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CineTrack.Models;

namespace CineTrack.Services
{
    /// <summary>
    /// Matches title, people and genre. Understands a few phrases used by the
    /// suggestion chips: "under 2 hours", "under 90 min", genre names.
    /// </summary>
    public static class MovieSearch
    {
        public static List<Movie> RunLocal(IEnumerable<Movie> catalog, string query)
        {
            var q = Normalize(query);
            if (q.Length == 0) return new();
            double? maxRuntime = null;
            var runtime = Regex.Match(q, @"(under|less than|shorter than)\s+(\d+(?:\.\d+)?)\s*(hours?|hrs?|h|minutes?|mins?|m)\b");
            if (runtime.Success)
            {
                var n = double.Parse(runtime.Groups[2].Value, CultureInfo.InvariantCulture);
                maxRuntime = runtime.Groups[3].Value.StartsWith("h") ? n * 60 : n;
                q = q.Remove(runtime.Index, runtime.Length).Trim();
            }

            var genre = catalog.SelectMany(m => m.Genres).Distinct().OrderByDescending(g => g.Length).FirstOrDefault(g => Regex.IsMatch(q, $@"\b{Regex.Escape(Normalize(g))}\b"));
            if (genre != null)
                q = Regex.Replace(q, $@"\b{Regex.Escape(Normalize(genre))}\b", "").Trim();

            q = Regex.Replace(q, @"\b(movies?|films?)\b", "").Trim();

            return catalog
                .Where(m => maxRuntime == null || m.RuntimeMin > 0 && m.RuntimeMin < maxRuntime)
                .Where(m => genre == null || m.Genres.Contains(genre))
                .Where(m => q.Length == 0 || TextMatches(m, q))
                .OrderByDescending(m => Normalize(m.Title).StartsWith(q) ? 1 : 0)
                .ThenByDescending(m => m.CommunityRating)
                .ToList();
        }

        static bool TextMatches(Movie m, string q)
        {
            var haystack = Normalize(string.Join(" | ",
                new[] { m.Title, m.Year.ToString() }.Concat(m.Genres).Concat(m.People)));
            return q.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(haystack.Contains);
        }

        /// <summary>Lowercase, strip accents and punctuation so "Hüller" matches "huller".</summary>
        static string Normalize(string s)
        {
            var decomposed = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '.' ? c : ' ');
            }
            return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }
    }
}
