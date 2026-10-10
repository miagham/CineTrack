using CineTrack.Models;
using CineTrack.Services;
using CineTrack.Web.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using System.Text.Json;

int checks = 0;
void Check(bool condition, string description)
{
    if (!condition) throw new Exception(description);
    checks++;
    Console.WriteLine($"PASS {description}");
}
var store = new WebMovieStore();
Check(store.Catalog.Count == 0 && store.TotalSaved == 0, "no hardcoded movies or user entries");
// Test fixtures exist only in this test project.
store.UpsertCatalog([
    new Movie { Id = "tmdb-1", TmdbId = 1, Title = "Arrival", Year = 2016, Genres = ["Science Fiction", "Drama"], RuntimeMin = 116, People = ["Denis Villeneuve", "Amy Adams"] },
    new Movie { Id = "tmdb-2", TmdbId = 2, Title = "Test Drama", Year = 2023, Genres = ["Drama"], RuntimeMin = 80, People = ["Sandra H\u00fcller"] }
]);
List<Movie> Search(string query) => MovieSearch.RunLocal(store.Catalog, query);
Check(Search("Arrival").Single().Id == "tmdb-1", "cached title search");
Check(Search("  ARRIVAL!! ").Single().Id == "tmdb-1", "case, punctuation and whitespace");
Check(Search("Denis Villeneuve").Count == 1, "cached director search");
Check(Search("Sandra Huller").Single().Id == "tmdb-2", "accent insensitive actor search");
Check(Search("2016").Single().Year == 2016, "cached year search");
Check(Search("Science Fiction").Single().Id == "tmdb-1", "API genre names used in cached search");
Check(Search("under 90 min").Single().Id == "tmdb-2", "runtime phrase search");
Check(Search("Drama under 2 hours").Count == 2, "combined genre and runtime");
Check(Search("").Count == 0 && Search("zzzxqnomovie").Count == 0, "empty and no results");
store.AddToWatchlist("tmdb-1"); var added = store.Entry("tmdb-1")!.AddedAt; store.AddToWatchlist("tmdb-1");
Check(store.TotalSaved == 1 && store.Entry("tmdb-1")!.AddedAt == added, "idempotent save");
store.SetRating("tmdb-1", 5); Check(store.Entry("tmdb-1")!.UserRating == null, "unwatched cannot be rated");
store.TogglePriority("tmdb-1"); Check(store.Entry("tmdb-1")!.HighPriority, "priority");
store.ToggleWatched("tmdb-1"); store.SetRating("tmdb-1", 4.3);
Check(store.MoviesWatched == 1 && store.Entry("tmdb-1")!.UserRating == 4.5 && store.HoursWatched == 2 && store.TopGenre == "Science Fiction", "watched rating and statistics");
store.UpsertCatalog([new Movie { Id = "tmdb-1", TmdbId = 1, Title = "Arrival", Genres = ["Science Fiction"] }]);
Check(store.HoursWatched == 2 && store.Get("tmdb-1")!.People.Contains("Denis Villeneuve"), "catalog refresh preserves saved full details and statistics");
store.SetRating("tmdb-1", double.NaN); Check(store.Entry("tmdb-1")!.UserRating == 4.5, "reject invalid rating");
store.RemoveFromWatchlist("tmdb-1"); Check(store.TotalSaved == 0 && store.MoviesWatched == 1 && !store.Entry("tmdb-1")!.HighPriority, "removal preserves history");
var json = JsonSerializer.Serialize(new Snapshot(store.Catalog.ToList(), store.Entries.ToList()));
var saved = JsonSerializer.Deserialize<Snapshot>(json)!; var restored = new WebMovieStore(); restored.Restore(saved.Movies, saved.Entries);
Check(restored.Entry("tmdb-1")!.UserRating == 4.5 && restored.Get("tmdb-1")!.Title == "Arrival", "API movies and entries survive serialization");
store.ToggleWatched("tmdb-1"); Check(store.Entry("tmdb-1")!.UserRating == null && store.MoviesWatched == 0, "unwatch clears rating");
store.AddToWatchlist("missing"); store.ToggleWatched("missing"); Check(store.Entry("missing") == null, "unknown IDs rejected");
Check(new WebMovieStore().TotalSaved == 0 && new WebMovieStore().Catalog.Count == 0, "collections isolated");
var handler = new FakeTmdb();
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["TMDB_READ_ACCESS_TOKEN"] = "test-only-token" }).Build();
var client = new TmdbClient(new HttpClient(handler), config);
var online = await client.SearchMoviesAsync("Arrival & Dune");
Check(online.Count == 1 && online[0].Id == "tmdb-42" && online[0].CommunityRating == 4 && online[0].Genres.Single() == "Drama", "API movie summaries mapped");
Check(online[0].PosterUrl == "https://image.tmdb.org/t/p/w500/test.jpg", "real poster URLs mapped");
Check(handler.Paths.Any(p => p.Contains("Arrival%20%26%20Dune")) && handler.Authorized, "query escaping and server authorization");
var details = (await client.MovieDetailsAsync(42))!;
Check(details.People.Contains("Test Director") && details.TrailerUrl == "https://www.youtube.com/watch?v=abc" && details.RuntimeMin == 100, "full cast trailer and runtime details");
Check((await client.GenresAsync())[18] == "Drama", "genres loaded from TMDB");
Check((await client.TrendingMoviesAsync()).Single().Id == "tmdb-42" && handler.Paths.Any(p => p.Contains("trending/movie/week")), "weekly trending endpoint");
await client.DiscoverMoviesAsync(18, topRated: true);
Check(handler.Paths.Any(p => p.Contains("with_genres=18") && p.Contains("sort_by=vote_average.desc") && p.Contains("vote_count.gte=500")), "genre and highly rated discovery filters");
await client.SearchMoviesAsync("Drama under 2 hours");
Check(handler.Paths.Last().Contains("with_runtime.lte=119") && handler.Paths.Last().Contains("with_genres=18"), "runtime and genre use discovery API");
await client.SearchMoviesAsync("2024");
Check(handler.Paths.Last().Contains("primary_release_year=2024"), "year uses discovery API");
await client.SearchMoviesAsync("Test Director");
Check(handler.Paths.Last().Contains("with_people=7") && handler.Paths.Any(p => p.Contains("search/person")), "person search uses people and discovery endpoints");
handler.Fail = true;
try { await client.TrendingMoviesAsync(); throw new Exception("Expected HTTP error"); }
catch (HttpRequestException) { Check(true, "TMDB failures propagate to visible error states"); }
var offline = new TmdbClient(new HttpClient(handler), new ConfigurationBuilder().Build());
Check((await offline.SearchMoviesAsync("test")).Count == 0 && (await offline.GenresAsync()).Count == 0 && (await offline.TrendingMoviesAsync()).Count == 0, "missing configuration makes no network calls");
Console.WriteLine($"All {checks} regression checks passed.");
record Snapshot(List<Movie> Movies, List<UserMovieEntry> Entries);
sealed class FakeTmdb : HttpMessageHandler
{
    public List<string> Paths = [];
    public bool Authorized, Fail;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = request.RequestUri!.OriginalString;
        Paths.Add(path);
        Authorized = request.Headers.Authorization?.Scheme == "Bearer";
        if (Fail) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        string json;
        if (path.Contains("genre/movie/list")) json = """{"genres":[{"id":18,"name":"Drama"},{"id":878,"name":"Science Fiction"}]}""";
        else if (path.Contains("search/person")) json = """{"results":[{"id":7,"name":"Test Director"}]}""";
        else if (path.Contains("search/movie") && path.Contains("Test%20Director")) json = """{"results":[]}""";
        else if (path.Contains("movie/42?")) json = """{"id":42,"title":"Test Movie","release_date":"2024-01-01","vote_average":8,"runtime":100,"poster_path":"/test.jpg","genres":[{"id":18,"name":"Drama"}],"credits":{"crew":[{"name":"Test Director","job":"Director"}],"cast":[{"name":"Test Actor"}]},"videos":{"results":[{"key":"abc","site":"YouTube","type":"Trailer","official":true}]}}""";
        else json = """{"results":[{"id":42,"title":"Test Movie","release_date":"2024-01-01","vote_average":8,"poster_path":"/test.jpg","genre_ids":[18]}]}""";
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
    }
}
