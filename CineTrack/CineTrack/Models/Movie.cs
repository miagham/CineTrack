namespace CineTrack.Models
{
    /// <summary>
    /// Catalog data for a single movie (seed data or, later, an external API).
    /// </summary>
    public class Movie
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public int Year { get; set; }
        public List<string> Genres { get; set; } = new();   // first entry = primary genre
        public int RuntimeMin { get; set; }                  // stored in minutes, shown as "2h 46m"
        public string? Certification { get; set; }           // "PG-13"
        public double CommunityRating { get; set; }          // 0–5, one decimal
        public string Synopsis { get; set; } = "";
        public string? PosterUrl { get; set; }               // optional; falls back to a gradient placeholder
        public string? BackdropUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public List<string> People { get; set; } = new();    // directors/actors, for search
        public List<string> Keywords { get; set; } = new();  // studios, awards ("A24", "Best Picture winner")
        public string Tint { get; set; } = "#35070B";        // placeholder poster color
        public bool EditorsPick { get; set; }                // small boost in hero recommendations

        public string PrimaryGenre => Genres.Count > 0 ? Genres[0] : "";
    }

    public static class GenreList
    {
        public static readonly string[] All =
        {
            "Action", "Comedy", "Drama", "Horror", "Sci-Fi",
            "Romance", "Thriller", "Animation", "Documentary", "Mystery"
        };
    }
}
