using System.IO;
using System.Text.Json;
using CineTrack.Models;

namespace CineTrack.Services
{
    /// <summary>
    /// Single source of truth for the catalog and the user's data. All user data is
    /// persisted to a local JSON file (never a server). Every mutation raises
    /// <see cref="Changed"/> so each screen and stat updates immediately.
    /// </summary>
    public sealed class MovieStore
    {
        const int DataVersion = 1;

        public static MovieStore Instance { get; } = new();

        readonly Dictionary<string, Movie> _catalog;
        Dictionary<string, UserMovieEntry> _entries = new();
        readonly string _dataPath;

        public event EventHandler? Changed;

        MovieStore()
        {
            var catalog = SeedData.Catalog();
            Catalog = catalog;
            _catalog = catalog.ToDictionary(m => m.Id);
            _dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CineTrack", "cinetrack-data.json");
            Load();
        }

        public IReadOnlyList<Movie> Catalog { get; }

        /// <summary>Where the user's data lives on this device.</summary>
        public string DataPath => _dataPath;

        public Movie? Get(string id) => _catalog.TryGetValue(id, out var m) ? m : null;

        public UserMovieEntry? Entry(string id) => _entries.TryGetValue(id, out var e) ? e : null;

        // ---------- Actions ----------

        public void AddToWatchlist(string id)
        {
            if (_entries.TryGetValue(id, out var e))
            {
                if (e.OnWatchlist) return;
                e.OnWatchlist = true;
                e.AddedAt = DateTime.Now;
            }
            else
            {
                _entries[id] = new UserMovieEntry
                {
                    MovieId = id,
                    Status = WatchStatus.WantToWatch,
                    OnWatchlist = true,
                    AddedAt = DateTime.Now,
                };
            }
            Commit();
        }

        public void MarkWatched(string id)
        {
            if (!_entries.TryGetValue(id, out var e))
            {
                e = new UserMovieEntry { MovieId = id, AddedAt = DateTime.Now };
                _entries[id] = e;
            }
            e.Status = WatchStatus.Watched;
            e.WatchedAt = DateTime.Now;
            Commit();
        }

        /// <summary>Returns a snapshot for undo.</summary>
        public UserMovieEntry? MarkUnwatched(string id)
        {
            if (!_entries.TryGetValue(id, out var e)) return null;
            var snapshot = e.Clone();
            if (e.OnWatchlist)
            {
                e.Status = WatchStatus.WantToWatch;
                e.WatchedAt = null;
                e.UserRating = null;
            }
            else
            {
                _entries.Remove(id);
            }
            Commit();
            return snapshot;
        }

        public void SetRating(string id, double? rating)
        {
            if (!_entries.TryGetValue(id, out var e)) return;
            e.UserRating = rating is null ? null : Math.Clamp(Math.Round(rating.Value * 2) / 2, 0.5, 5);
            Commit();
        }

        public bool TogglePriority(string id)
        {
            if (!_entries.TryGetValue(id, out var e) || !e.OnWatchlist) return false;
            e.HighPriority = !e.HighPriority;
            Commit();
            return e.HighPriority;
        }

        /// <summary>Removes from the watchlist; watched movies stay in history. Returns a snapshot for undo.</summary>
        public UserMovieEntry? RemoveFromWatchlist(string id)
        {
            if (!_entries.TryGetValue(id, out var e)) return null;
            var snapshot = e.Clone();
            if (e.IsWatched)
            {
                e.OnWatchlist = false;
                e.HighPriority = false;
            }
            else
            {
                _entries.Remove(id);
            }
            Commit();
            return snapshot;
        }

        public void Restore(UserMovieEntry snapshot)
        {
            _entries[snapshot.MovieId] = snapshot.Clone();
            Commit();
        }

        public Dictionary<string, UserMovieEntry> SnapshotAll() =>
            _entries.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());

        public void RestoreAll(Dictionary<string, UserMovieEntry> snapshot)
        {
            _entries = snapshot.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());
            Commit();
        }

        public void ClearAll()
        {
            _entries.Clear();
            Commit();
        }

        public void ResetToSampleData()
        {
            _entries = SeedData.UserEntries(DateTime.Now).ToDictionary(e => e.MovieId);
            Commit();
        }

        // ---------- Derived values (computed, never stored) ----------

        public IEnumerable<UserMovieEntry> Entries => _entries.Values;

        public IEnumerable<UserMovieEntry> WatchlistEntries => _entries.Values.Where(e => e.OnWatchlist);

        public int TotalSaved => WatchlistEntries.Count();
        public int WatchlistWatchedCount => WatchlistEntries.Count(e => e.IsWatched);
        public int WatchlistUnwatchedCount => WatchlistEntries.Count(e => !e.IsWatched);
        public int HighPriorityCount => WatchlistEntries.Count(e => e.HighPriority);

        public int TimeToClearMinutes => WatchlistEntries
            .Where(e => !e.IsWatched)
            .Sum(e => Get(e.MovieId)?.RuntimeMin ?? 0);

        public IEnumerable<UserMovieEntry> WatchedEntries => _entries.Values.Where(e => e.IsWatched);

        public int MoviesWatched => WatchedEntries.Count();
        public int WantToWatchCount => _entries.Values.Count(e => e.Status == WatchStatus.WantToWatch);

        public int HoursWatched => (int)Math.Round(
            WatchedEntries.Sum(e => Get(e.MovieId)?.RuntimeMin ?? 0) / 60.0, MidpointRounding.AwayFromZero);

        /// <summary>Most frequent primary genre among watched movies; ties go to the most recently watched.</summary>
        public string? TopGenre => WatchedEntries
            .Select(e => (Movie: Get(e.MovieId), Entry: e))
            .Where(x => x.Movie != null)
            .GroupBy(x => x.Movie!.PrimaryGenre)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Max(x => x.Entry.WatchedAt ?? DateTime.MinValue))
            .Select(g => g.Key)
            .FirstOrDefault();

        /// <summary>Watchlist entries grouped by primary genre, largest first.</summary>
        public List<(string Genre, int Count)> WatchlistGenreCounts() => WatchlistEntries
            .Select(e => Get(e.MovieId)?.PrimaryGenre)
            .Where(g => g != null)
            .GroupBy(g => g!)
            .Select(g => (g.Key, g.Count()))
            .OrderByDescending(x => x.Item2)
            .ThenBy(x => x.Key)
            .ToList();

        public List<UserMovieEntry> RecentHistory(int count) => WatchedEntries
            .OrderByDescending(e => e.WatchedAt ?? DateTime.MinValue)
            .Take(count)
            .ToList();

        /// <summary>"Must-sees up front": high-priority unwatched, other unwatched, then watched; each newest first.</summary>
        public List<UserMovieEntry> WatchlistDefaultOrder() => WatchlistEntries
            .OrderBy(e => e.IsWatched ? 2 : e.HighPriority ? 0 : 1)
            .ThenByDescending(e => e.AddedAt)
            .ToList();

        /// <summary>Highly rated, unwatched, not on the watchlist, weighted toward the user's top genre.</summary>
        public List<Movie> Recommendations(int count)
        {
            var top = TopGenre;
            var picks = Catalog
                .Where(m => Entry(m.Id) == null)
                .OrderByDescending(m => m.CommunityRating
                    + (top != null && m.Genres.Contains(top) ? 0.3 : 0)
                    + (m.EditorsPick ? 0.5 : 0))
                .Take(count)
                .ToList();
            return picks.Count > 0
                ? picks
                : Catalog.OrderByDescending(m => m.CommunityRating).Take(count).ToList();
        }

        // ---------- Persistence ----------

        sealed class DataFile
        {
            public int Version { get; set; }
            public List<UserMovieEntry> Entries { get; set; } = new();
        }

        static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        void Load()
        {
            try
            {
                if (File.Exists(_dataPath))
                {
                    var data = JsonSerializer.Deserialize<DataFile>(File.ReadAllText(_dataPath), JsonOptions);
                    if (data != null && data.Version == DataVersion)
                    {
                        _entries = data.Entries
                            .Where(e => _catalog.ContainsKey(e.MovieId))
                            .GroupBy(e => e.MovieId)
                            .ToDictionary(g => g.Key, g => g.Last());
                        return;
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                // Unreadable data falls back to the sample data below.
            }

            _entries = SeedData.UserEntries(DateTime.Now).ToDictionary(e => e.MovieId);
            Save();
        }

        void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_dataPath)!);
                var data = new DataFile { Version = DataVersion, Entries = _entries.Values.ToList() };
                var tmp = _dataPath + ".tmp";
                File.WriteAllText(tmp, JsonSerializer.Serialize(data, JsonOptions));
                File.Move(tmp, _dataPath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Keep working in memory if the disk is unavailable.
            }
        }

        void Commit()
        {
            Save();
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
