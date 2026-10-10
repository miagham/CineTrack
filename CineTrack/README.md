# CineTrack

A responsive Blazor website for discovering films, searching TMDB, maintaining a watchlist, recording watched movies and ratings, and viewing personal statistics. Collections are browser-local.

```powershell
dotnet run --project CineTrack.Web/CineTrack.Web.csproj
```

To load movies and images, set your TMDB **API Read Access Token** in the server environment before starting (use the Read Access Token, not the v3 API key):

```powershell
$env:TMDB_READ_ACCESS_TOKEN = 'your-read-access-token'
dotnet run --project CineTrack.Web/CineTrack.Web.csproj
```

Restart the server after changing the token. Discover, Genres, search, posters and details use TMDB. The application contains no hardcoded movie catalog. Without a token, saved movies remain accessible and cached search is available.

## Verification

```powershell
dotnet build CineTrack.sln -c Release
dotnet run --project tests/CineTrack.Checks/CineTrack.Checks.csproj -c Release
```

The regression runner exits with a failure on any failed check and covers search, collection changes, ratings, persistence serialization, isolation, TMDB trending/discovery/people search, image mapping and error responses.

Browser checks require a configured TMDB token, Node.js 22 or later and Chrome. Start the app at `http://127.0.0.1:5088`, set `CHROME_PATH` to your Chrome executable, then run:

```powershell
$env:CHROME_PATH = 'C:/Program Files/Google/Chrome/Application/chrome.exe'
node tests/browser.mjs
```

The browser suite verifies live trending, genre discovery, image downloads, search, movie details, trailer links and saved online movies.

`BASE_URL` can override the local server URL. The browser runner uses its own profile under ignored `artifacts/`, clears that profile's site collection, and tests navigation, search, saving, priority, watched status, ratings, refresh persistence, removal, genres, statistics, empty states and mobile width.

See [Azure deployment instructions](DEPLOY_AZURE.md).

TMDB endpoints: [weekly trending](https://developer.themoviedb.org/reference/trending-movies), [discovery filters](https://developer.themoviedb.org/reference/discover-movie), [people search](https://developer.themoviedb.org/reference/search-person), and [image URLs](https://developer.themoviedb.org/docs/image-basics).
