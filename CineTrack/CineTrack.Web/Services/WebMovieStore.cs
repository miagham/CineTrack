using CineTrack.Models;

namespace CineTrack.Web.Services;

public sealed class WebMovieStore
{
    readonly List<Movie> _catalog = new();
    readonly Dictionary<string, UserMovieEntry> _entries = new();

    public void UpsertCatalog(IEnumerable<Movie> movies)
    {
        foreach (var movie in movies)
        {
            var existing = Get(movie.Id);
            if (existing != null)
            {
                if (movie.RuntimeMin == 0) movie.RuntimeMin = existing.RuntimeMin;
                if (movie.People.Count == 0) movie.People = existing.People;
                movie.Certification ??= existing.Certification;
                movie.TrailerUrl ??= existing.TrailerUrl;
            }
            _catalog.RemoveAll(m => m.Id == movie.Id);
            _catalog.Add(movie);
        }
    }
    public void Restore(IEnumerable<Movie> movies, IEnumerable<UserMovieEntry> entries)
    {
        UpsertCatalog(movies.Where(m => m.TmdbId is > 0));
        _entries.Clear();
        foreach (var entry in entries.Where(e => Get(e.MovieId) != null)) _entries[entry.MovieId] = entry;
        Changed?.Invoke();
    }

    public event Action? Changed;

    public IReadOnlyList<Movie> Catalog => _catalog;
    public IEnumerable<UserMovieEntry> Entries => _entries.Values;
    public IEnumerable<UserMovieEntry> WatchlistEntries => _entries.Values.Where(e => e.OnWatchlist);
    public IEnumerable<UserMovieEntry> WatchedEntries => _entries.Values.Where(e => e.IsWatched);

    public int TotalSaved => WatchlistEntries.Count();
    public int MoviesWatched => WatchedEntries.Count();
    public int HoursWatched => (int)Math.Round(WatchedEntries.Sum(e => Get(e.MovieId)?.RuntimeMin ?? 0) / 60.0, MidpointRounding.AwayFromZero);

    public string? TopGenre => WatchedEntries
        .Select(e => (Movie: Get(e.MovieId), Entry: e))
        .Where(x => x.Movie is not null)
        .GroupBy(x => x.Movie!.PrimaryGenre)
        .OrderByDescending(g => g.Count())
        .ThenByDescending(g => g.Max(x => x.Entry.WatchedAt ?? DateTime.MinValue))
        .Select(g => g.Key)
        .FirstOrDefault();

    public Movie? Get(string id) => _catalog.FirstOrDefault(m => m.Id == id);
    public UserMovieEntry? Entry(string id) => _entries.TryGetValue(id, out var e) ? e : null;

    public void AddToWatchlist(string id)
    {
        if (Get(id) == null || Entry(id)?.OnWatchlist == true) return;
        if (_entries.TryGetValue(id, out var entry))
        {
            entry.OnWatchlist = true;
            entry.AddedAt = DateTime.Now;
            if (!entry.IsWatched) entry.Status = WatchStatus.WantToWatch;
        }
        else
        {
            _entries[id] = new UserMovieEntry
            {
                MovieId = id,
                Status = WatchStatus.WantToWatch,
                OnWatchlist = true,
                AddedAt = DateTime.Now
            };
        }

        Changed?.Invoke();
    }

    public void ToggleWatched(string id)
    {
        if (Get(id) == null) return;
        if (!_entries.TryGetValue(id, out var entry))
        {
            entry = new UserMovieEntry { MovieId = id, AddedAt = DateTime.Now };
            _entries[id] = entry;
        }

        if (entry.IsWatched)
        {
            entry.Status = WatchStatus.WantToWatch;
            entry.WatchedAt = null;
            entry.UserRating = null;
        }
        else
        {
            entry.Status = WatchStatus.Watched;
            entry.WatchedAt = DateTime.Now;
        }

        Changed?.Invoke();
    }

    public void RemoveFromWatchlist(string id)
    {
        if (!_entries.TryGetValue(id, out var entry)) return;
        if (entry.IsWatched)
        {
            entry.OnWatchlist = false;
            entry.HighPriority = false;
        }
        else
        {
            _entries.Remove(id);
        }

        Changed?.Invoke();
    }

    public void TogglePriority(string id)
    {
        if (_entries.TryGetValue(id, out var entry) && entry.OnWatchlist)
        {
            entry.HighPriority = !entry.HighPriority;
            Changed?.Invoke();
        }
    }

    public void SetRating(string id, double rating)
    {
        if (!double.IsFinite(rating) || !_entries.TryGetValue(id, out var entry) || !entry.IsWatched) return;
        entry.UserRating = Math.Clamp(Math.Round(rating * 2) / 2, 0.5, 5);
        Changed?.Invoke();
    }

    public List<UserMovieEntry> RecentHistory(int count) => WatchedEntries
        .OrderByDescending(e => e.WatchedAt ?? DateTime.MinValue)
        .Take(count)
        .ToList();

    public List<UserMovieEntry> WatchlistDefaultOrder() => WatchlistEntries
        .OrderBy(e => e.IsWatched ? 2 : e.HighPriority ? 0 : 1)
        .ThenByDescending(e => e.AddedAt)
        .ToList();

}
