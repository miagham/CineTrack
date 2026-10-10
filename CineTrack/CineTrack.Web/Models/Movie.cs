namespace CineTrack.Models
{
    /// <summary>
    /// Movie information loaded from TMDB.
    /// </summary>
    public class Movie
    {
        public string Id { get; set; } = "";
        public int? TmdbId { get; set; }
        public string Title { get; set; } = "";
        public int Year { get; set; }
        public List<string> Genres { get; set; } = new();   // first entry = primary genre
        public int RuntimeMin { get; set; }                  // stored in minutes, shown as "2h 46m"
        public string? Certification { get; set; }           // "PG-13"
        public double CommunityRating { get; set; }          // 0–5, one decimal
        public string Synopsis { get; set; } = "";
        public string? PosterUrl { get; set; }               // optional; falls back to a gradient placeholder
        public string? TrailerUrl { get; set; }
        public List<string> People { get; set; } = new();    // directors/actors, for search
        public string Tint { get; set; } = "#35070B";        // placeholder poster color

        public string PrimaryGenre => Genres.Count > 0 ? Genres[0] : "";
    }

}
