using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CineTrack.Models;

namespace CineTrack.Services
{
    /// <summary>
    /// Matches title, people, genre and keywords. Understands a few phrases used by the
    /// suggestion chips: "under 2 hours", "under 90 min", "Best Picture winners", genre names.
    /// </summary>
    public static class MovieSearch
    {
        public static List<Movie> Run(string query)
        {
            var q = Normalize(query);
            if (q.Length == 0) return new();

            int? maxRuntime = null;
            var runtime = Regex.Match(q, @"(under|less than|shorter than)\s+(\d+(?:\.\d+)?)\s*(hours?|hrs?|h|minutes?|mins?|m)\b");
            if (runtime.Success)
            {
                var n = double.Parse(runtime.Groups[2].Value, CultureInfo.InvariantCulture);
                maxRuntime = (int)(runtime.Groups[3].Value.StartsWith("h") ? n * 60 : n);
                q = q.Remove(runtime.Index, runtime.Length).Trim();
            }

            var genre = GenreList.All.FirstOrDefault(g => Regex.IsMatch(q, $@"\b{Regex.Escape(Normalize(g))}\b"));
            if (genre != null)
                q = Regex.Replace(q, $@"\b{Regex.Escape(Normalize(genre))}\b", "").Trim();

            if (q.EndsWith(" winners")) q = q[..^1];   // "best picture winners" → "best picture winner"
            q = Regex.Replace(q, @"\b(movies?|films?)\b", "").Trim();

            return MovieStore.Instance.Catalog
                .Where(m => maxRuntime == null || m.RuntimeMin < maxRuntime)
                .Where(m => genre == null || m.Genres.Contains(genre))
                .Where(m => q.Length == 0 || TextMatches(m, q))
                .OrderByDescending(m => Normalize(m.Title).StartsWith(q) ? 1 : 0)
                .ThenByDescending(m => m.CommunityRating)
                .ToList();
        }

        static bool TextMatches(Movie m, string q)
        {
            var haystack = Normalize(string.Join(" | ",
                new[] { m.Title, m.Year.ToString() }.Concat(m.Genres).Concat(m.People).Concat(m.Keywords)));
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
