using System.Text.Json.Serialization;

namespace CineTrack.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WatchStatus
    {
        WantToWatch,
        Watched
    }

    /// <summary>
    /// The user's relationship with one movie. Stored locally on the device.
    /// </summary>
    public class UserMovieEntry
    {
        public string MovieId { get; set; } = "";
        public WatchStatus Status { get; set; }
        public bool OnWatchlist { get; set; }      // stays true after watching unless removed
        public bool HighPriority { get; set; }
        public double? UserRating { get; set; }    // 0.5–5, set when watched
        public DateTime AddedAt { get; set; }
        public DateTime? WatchedAt { get; set; }

        [JsonIgnore]
        public bool IsWatched =>Status == WatchStatus.Watched;

        public UserMovieEntry Clone() => (UserMovieEntry)MemberwiseClone();
    }
}
