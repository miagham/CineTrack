using CineTrack.Models;

namespace CineTrack.Services
{
    /// <summary>UI text formats from spec section 3.2.</summary>
    public static class Format
    {
        public const string Dot = " · ";

        /// <summary>166 → "2h 46m", 120 → "2h 0m", 45 → "45m".</summary>
        public static string Runtime(int minutes) =>
            minutes < 60 ? $"{minutes}m" : $"{minutes / 60}h {minutes % 60}m";

        /// <summary>Same as <see cref="Runtime"/>; used for totals like "12h 5m".</summary>
        public static string Duration(int minutes) => minutes == 0 ? "0h 0m" : Runtime(minutes);

        public static string Meta(Movie m, bool withRuntime = false) =>
            withRuntime ? $"{m.Year}{Dot}{m.PrimaryGenre}{Dot}{Runtime(m.RuntimeMin)}" : $"{m.Year}{Dot}{m.PrimaryGenre}";

        public static string Rating(double r) => r.ToString("0.0");

        public static string RelativeTime(DateTime when)
        {
            var days = (DateTime.Now.Date - when.Date).Days;
            if (days <= 0) return "today";
            if (days == 1) return "yesterday";
            if (days < 7) return $"{days} days ago";
            if (days < 30) return days < 14 ? "1 week ago" : $"{days / 7} weeks ago";
            if (days < 365) return days < 60 ? "1 month ago" : $"{days / 30} months ago";
            return days < 730 ? "1 year ago" : $"{days / 365} years ago";
        }

        public static string Plural(int n, string one, string many) => n == 1 ? $"{n} {one}" : $"{n} {many}";
    }
}
