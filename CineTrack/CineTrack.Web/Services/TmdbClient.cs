using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CineTrack.Models;

namespace CineTrack.Services
{
    /// <summary>
    /// Small TMDB API client. It uses the API Read Access Token from
    /// TMDB_READ_ACCESS_TOKEN and maps TMDB data into CineTrack's Movie model.
    /// </summary>
    public sealed class TmdbClient
    {
        const string PosterBaseUrl = "https://image.tmdb.org/t/p/w500";

        static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        readonly HttpClient _http;
        readonly System.Collections.Concurrent.ConcurrentDictionary<int, Movie> _detailsCache = new();
        Dictionary<int, string>? _genresById;



        public bool IsConfigured { get; }

        public TmdbClient(HttpClient http, IConfiguration configuration)
        {
            var token = configuration["TMDB_READ_ACCESS_TOKEN"];
            IsConfigured = !string.IsNullOrWhiteSpace(token);
            _http = http;
            _http.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            _http.Timeout = TimeSpan.FromSeconds(15);
            if (IsConfigured) _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<Dictionary<int, string>> GenresAsync(CancellationToken cancellationToken = default)
        {
            if (!IsConfigured) return new();
            return new Dictionary<int, string>(await GenresByIdAsync(cancellationToken));
        }

        public async Task<List<Movie>> TrendingMoviesAsync(CancellationToken cancellationToken = default)
        {
            if (!IsConfigured) return new();
            var response = await GetAsync<TmdbPaged<TmdbMovieSummary>>("trending/movie/week?language=en-US", cancellationToken);
            return await MapSummariesAsync(response.Results, cancellationToken);
        }

        public async Task<List<Movie>> DiscoverMoviesAsync(int? genreId = null, int? maxRuntime = null,
            int? year = null, int? personId = null, bool topRated = false, CancellationToken cancellationToken = default)
        {
            if (!IsConfigured) return new();
            var path = "discover/movie?include_adult=false&include_video=false&language=en-US&page=1&sort_by=popularity.desc";
            if (topRated) path = path.Replace("sort_by=popularity.desc", "sort_by=vote_average.desc&vote_count.gte=500");
            if (genreId.HasValue) path += $"&with_genres={genreId.Value}";
            if (maxRuntime.HasValue) path += $"&with_runtime.gte=1&with_runtime.lte={maxRuntime.Value}";
            if (year.HasValue) path += $"&primary_release_year={year.Value}";
            if (personId.HasValue) path += $"&with_people={personId.Value}";
            var response = await GetAsync<TmdbPaged<TmdbMovieSummary>>(path, cancellationToken);
            return await MapSummariesAsync(response.Results, cancellationToken);
        }

        public async Task<List<Movie>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default)
        {
            if (!IsConfigured || string.IsNullOrWhiteSpace(query)) return new();
            query = query.Trim();
            var genres = await GenresByIdAsync(cancellationToken);
            var runtime = System.Text.RegularExpressions.Regex.Match(query,
                @"(?:under|less than|shorter than)\s+(\d+(?:\.\d+)?)\s*(hours?|hrs?|h|minutes?|mins?|m)\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            int? maxRuntime = null;
            if (runtime.Success)
            {
                var amount = double.Parse(runtime.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                var minutes = runtime.Groups[2].Value.StartsWith("h", StringComparison.OrdinalIgnoreCase) ? amount * 60 : amount;
                maxRuntime = (int)Math.Clamp(Math.Ceiling(minutes) - 1, 0, int.MaxValue);
                query = query.Remove(runtime.Index, runtime.Length).Trim();
            }
            query = System.Text.RegularExpressions.Regex.Replace(query, @"\b(movies?|films?)\b", "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            var genreName = query.Equals("Sci-Fi", StringComparison.OrdinalIgnoreCase) ? "Science Fiction" : query;
            var genre = genres.FirstOrDefault(g => g.Value.Equals(genreName, StringComparison.OrdinalIgnoreCase));
            var isYear = int.TryParse(query, out var year) && year >= 1870 && year <= 2200;
            if (genre.Key != 0 || isYear || query.Length == 0)
                return await DiscoverMoviesAsync(genre.Key == 0 ? null : genre.Key, maxRuntime, isYear ? year : null,
                    cancellationToken: cancellationToken);

            var movieRequest = GetAsync<TmdbPaged<TmdbMovieSummary>>(
                $"search/movie?query={Uri.EscapeDataString(query)}&include_adult=false&language=en-US&page=1", cancellationToken);
            var peopleRequest = GetAsync<TmdbPaged<TmdbPerson>>(
                $"search/person?query={Uri.EscapeDataString(query)}&include_adult=false&language=en-US&page=1", cancellationToken);
            await Task.WhenAll(movieRequest, peopleRequest);
            var summaries = (await movieRequest).Results;
            var people = (await peopleRequest).Results;
            var person = people.FirstOrDefault(p => System.Globalization.CultureInfo.InvariantCulture.CompareInfo.Compare(
                p.Name, query, System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace) == 0);
            person ??= summaries.Count == 0 ? people.FirstOrDefault() : null;
            if (person != null) return await DiscoverMoviesAsync(maxRuntime: maxRuntime, personId: person.Id,
                cancellationToken: cancellationToken);
            var movies = await MapSummariesAsync(summaries, cancellationToken);
            if (maxRuntime.HasValue)
            {
                var detailed = await Task.WhenAll(movies.Select(m => MovieDetailsAsync(m.TmdbId!.Value, cancellationToken)));
                return detailed.Where(m => m != null && m.RuntimeMin > 0 && m.RuntimeMin <= maxRuntime.Value).Cast<Movie>().ToList();
            }
            return movies;
        }

        public async Task<Movie?> MovieDetailsAsync(int tmdbId, CancellationToken cancellationToken = default)
        {
            if (!IsConfigured) return null;
            if (_detailsCache.TryGetValue(tmdbId, out var cached)) return cached;

            var dto = await GetAsync<TmdbMovieDetails>(
                $"movie/{tmdbId}?language=en-US&append_to_response=credits,videos,release_dates",
                cancellationToken);

            var movie = MapDetails(dto);
            _detailsCache[tmdbId] = movie;
            return movie;
        }

        async Task<List<Movie>> MapSummariesAsync(IEnumerable<TmdbMovieSummary> summaries, CancellationToken cancellationToken)
        {
            await GenresByIdAsync(cancellationToken);
            var selected = summaries.Where(m => m.Id > 0 && !string.IsNullOrWhiteSpace(m.Title)).Take(18);
            return (await Task.WhenAll(selected.Select(m => MapSummaryAsync(m, cancellationToken)))).ToList();
        }

        async Task<Movie> MapSummaryAsync(TmdbMovieSummary dto, CancellationToken cancellationToken)
        {
            var genres = await GenreNamesAsync(dto.GenreIds, cancellationToken);
            return new Movie
            {
                Id = TmdbMovieId(dto.Id),
                TmdbId = dto.Id,
                Title = dto.Title ?? dto.OriginalTitle ?? "Untitled",
                Year = YearFromDate(dto.ReleaseDate),
                Genres = genres,
                RuntimeMin = 0,
                Certification = null,
                CommunityRating = ToFiveStarRating(dto.VoteAverage),
                Synopsis = string.IsNullOrWhiteSpace(dto.Overview) ? "No description is available yet." : dto.Overview,
                PosterUrl = ImageUrl(PosterBaseUrl, dto.PosterPath),
                Tint = TintForId(dto.Id),
            };
        }

        Movie MapDetails(TmdbMovieDetails dto)
        {
            var director = dto.Credits?.Crew?.FirstOrDefault(c => c.Job == "Director")?.Name;
            var cast = dto.Credits?.Cast?
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .Take(4)
                .Select(c => c.Name!)
                .ToList() ?? new List<string>();
            var people = new List<string>();
            if (!string.IsNullOrWhiteSpace(director)) people.Add(director);
            people.AddRange(cast);

            var trailerKey = dto.Videos?.Results?
                .Where(v => v.Site == "YouTube" && v.Type == "Trailer" && !string.IsNullOrWhiteSpace(v.Key))
                .OrderByDescending(v => v.Official)
                .ThenByDescending(v => v.PublishedAt)
                .Select(v => v.Key)
                .FirstOrDefault();

            return new Movie
            {
                Id = TmdbMovieId(dto.Id),
                TmdbId = dto.Id,
                Title = dto.Title ?? dto.OriginalTitle ?? "Untitled",
                Year = YearFromDate(dto.ReleaseDate),
                Genres = dto.Genres?.Select(g => g.Name).Where(g => !string.IsNullOrWhiteSpace(g)).Cast<string>().ToList()
                    ?? new List<string>(),
                RuntimeMin = dto.Runtime,
                Certification = UsCertification(dto.ReleaseDates),
                CommunityRating = ToFiveStarRating(dto.VoteAverage),
                Synopsis = string.IsNullOrWhiteSpace(dto.Overview) ? "No description is available yet." : dto.Overview,
                PosterUrl = ImageUrl(PosterBaseUrl, dto.PosterPath),
                TrailerUrl = trailerKey == null ? null : $"https://www.youtube.com/watch?v={trailerKey}",
                People = people,
                Tint = TintForId(dto.Id),
            };
        }

        async Task<List<string>> GenreNamesAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            var all = await GenresByIdAsync(cancellationToken);
            return ids.Select(id => all.TryGetValue(id, out var name) ? name : null)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .ToList();
        }

        async Task<Dictionary<int, string>> GenresByIdAsync(CancellationToken cancellationToken)
        {
            if (_genresById != null) return _genresById;
            var response = await GetAsync<TmdbGenresResponse>("genre/movie/list?language=en-US", cancellationToken);
            _genresById = response.Genres.ToDictionary(g => g.Id, g => g.Name);
            return _genresById;
        }

        async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
        {
            using var response = await _http.GetAsync(path, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
                ?? throw new JsonException($"TMDB returned an empty response for {path}.");
        }

        static string TmdbMovieId(int id) => $"tmdb-{id}";

        static string? ImageUrl(string baseUrl, string? path) =>
            string.IsNullOrWhiteSpace(path) ? null : baseUrl + path;

        static int YearFromDate(string? date) =>
            DateTime.TryParse(date, out var parsed) ? parsed.Year : 0;

        static double ToFiveStarRating(double voteAverage) =>
            Math.Round(Math.Clamp(voteAverage / 2, 0, 5), 1);

        static string TintForId(int id)
        {
            var palette = new[] { "#3E4A57", "#B07A38", "#8A4B1E", "#4F6B78", "#6B3F7A", "#56606A", "#A34F24", "#2B4C8C" };
            return palette[Math.Abs(id) % palette.Length];
        }

        static string? UsCertification(TmdbReleaseDates? releaseDates)
        {
            return releaseDates?.Results?
                .FirstOrDefault(r => r.Iso31661 == "US")?
                .ReleaseDates?
                .Select(r => r.Certification)
                .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));
        }

        sealed class TmdbPaged<T>
        {
            public List<T> Results { get; set; } = new();
        }

        sealed class TmdbPerson
        {
            public string Name { get; set; } = "";
            public int Id { get; set; }
        }

        sealed class TmdbMovieSummary
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            [JsonPropertyName("original_title")] public string? OriginalTitle { get; set; }
            public string? Overview { get; set; }
            [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
            [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
            [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
            [JsonPropertyName("genre_ids")] public List<int> GenreIds { get; set; } = new();
        }

        sealed class TmdbMovieDetails
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            [JsonPropertyName("original_title")] public string? OriginalTitle { get; set; }
            public string? Overview { get; set; }
            [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
            [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
            [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
            public int Runtime { get; set; }
            public List<TmdbGenre>? Genres { get; set; }
            public TmdbCredits? Credits { get; set; }
            public TmdbVideos? Videos { get; set; }
            [JsonPropertyName("release_dates")] public TmdbReleaseDates? ReleaseDates { get; set; }
        }

        sealed class TmdbGenre
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        sealed class TmdbGenresResponse
        {
            public List<TmdbGenre> Genres { get; set; } = new();
        }

        sealed class TmdbCredits
        {
            public List<TmdbCast>? Cast { get; set; }
            public List<TmdbCrew>? Crew { get; set; }
        }

        sealed class TmdbCast
        {
            public string? Name { get; set; }
        }

        sealed class TmdbCrew
        {
            public string? Name { get; set; }
            public string? Job { get; set; }
        }

        sealed class TmdbVideos
        {
            public List<TmdbVideo>? Results { get; set; }
        }

        sealed class TmdbVideo
        {
            public string? Key { get; set; }
            public string? Site { get; set; }
            public string? Type { get; set; }
            public bool Official { get; set; }
            [JsonPropertyName("published_at")] public DateTime? PublishedAt { get; set; }
        }

        sealed class TmdbReleaseDates
        {
            public List<TmdbReleaseCountry>? Results { get; set; }
        }

        sealed class TmdbReleaseCountry
        {
            [JsonPropertyName("iso_3166_1")] public string? Iso31661 { get; set; }
            [JsonPropertyName("release_dates")] public List<TmdbReleaseCertification>? ReleaseDates { get; set; }
        }

        sealed class TmdbReleaseCertification
        {
            public string? Certification { get; set; }
        }
    }
}
