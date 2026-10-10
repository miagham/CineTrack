# Deploy CineTrack to Azure App Service

CineTrack is a .NET 8 Blazor Web App using Interactive Server rendering. Deploy the server project to **Azure App Service**, with a running .NET backend. JavaScript and an active connection to the server are required.

## 1. Verify locally

Install the .NET 8 SDK (a newer SDK capable of targeting net8.0 also works). From the repository directory:

```powershell
dotnet build CineTrack.sln -c Release
dotnet run --project tests/CineTrack.Checks/CineTrack.Checks.csproj -c Release
dotnet run --project CineTrack.Web/CineTrack.Web.csproj
```

Open the URL printed by the server. Movie data and posters require the server environment variable `TMDB_READ_ACCESS_TOKEN`, containing a TMDB API Read Access Token. Set it before launching the app and restart after changing it. Keep the token out of source files and browser code. The homepage and genre browser load directly from TMDB. Without a token, they show a configuration message; saved movies remain accessible. Search supports titles, people, genres, years and runtime phrases. An online failure shows matches from previously loaded movies and a status message.

## 2. Create the Azure resource

In Azure Portal, create a **Web App** with:

- Publish: Code.
- Runtime: .NET 8 (LTS).
- Operating system: Windows, for the instructions below.
- An App Service plan such as Basic B1 that supports Always On.
- A globally unique app name and a region near your visitors.

Under Configuration / General settings (or Settings / Configuration, depending on the portal layout), enable **WebSockets**, **ARR affinity / Session affinity**, and **Always On**. Enable **HTTPS Only**. Keep one instance for the initial deployment. In Environment variables / App settings, set `ASPNETCORE_ENVIRONMENT` to `Production`. Add `TMDB_READ_ACCESS_TOKEN` using its real secret value in the portal, then save and restart. Never commit the token.

Interactive Server uses a persistent SignalR connection. Microsoft documents the [WebSockets and session affinity settings](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server/?view=aspnetcore-8.0). For multiple instances, review that guide's scaling requirements before scaling out.

## 3. Publish and deploy

Install Azure CLI and sign in. Run these PowerShell commands from the repository directory. Replace the resource group and app name with your Azure resource values:

```powershell
az login
$resourceGroup = 'your-resource-group'
$appName = 'your-globally-unique-app-name'

dotnet publish CineTrack.Web/CineTrack.Web.csproj -c Release -o artifacts/azure-publish
if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
Compress-Archive -Path ./artifacts/azure-publish/* -DestinationPath ./artifacts/cinetrack.zip -Force
az webapp deploy --resource-group $resourceGroup --name $appName --src-path ./artifacts/cinetrack.zip --type zip
```

The ZIP must contain the published files directly at its root, including `CineTrack.Web.dll` and `wwwroot`; avoid wrapping them in an extra directory. [Microsoft's ZIP deployment instructions](https://learn.microsoft.com/en-us/azure/app-service/deploy-zip) explain this format and the CLI command. Deploying restarts the app, so open browser sessions may need a reload.

Alternatively, in Visual Studio right-click **CineTrack.Web** > Publish > Azure > Azure App Service (Windows), select the Web App, and publish with Release configuration.

## 4. Verify the deployment

Open the default domain shown on your Azure Web App overview page, usually `https://<app-name>.azurewebsites.net`.

1. Visit `/health`; expect JSON with `status` equal to `healthy`.
2. Search for `Arrival`, `Denis Villeneuve`, `Science Fiction`, and `under 2 hours`. Confirm results, and confirm an unmatched query shows the empty state.
3. Confirm Discover loads trending movies and images. Search for a title and open its details. Check its poster, cast and available trailer link.
4. Add a movie to Watchlist, set priority, mark watched, and choose a rating on its details page.
5. Refresh, then check Watchlist, My movies and Profile. Removing a watched movie from Watchlist should preserve it in My movies. Marking it unwatched clears its rating.
6. Check navigation and genre filtering on a phone-sized screen.

## Storage and operational notes

Collections are saved in browser localStorage, separately for each browser and site origin. They survive server restarts and deployments, but do not sync between devices. Clearing site data removes them. There is no account system or Azure database to provision. Moving from localhost to Azure starts a separate collection. Concurrent browser tabs can overwrite each other's last saved collection.

Blazor accepts browser storage payloads up to 1 MiB. Only saved TMDB movie details are persisted alongside entries; movie listings and genres are fetched from TMDB. Very large collections may exceed this limit and need a server-backed database in a future version.

If the page loads but buttons do not work, check WebSockets, session affinity, and the browser's `_blazor` connection. For startup errors, use App Service Log stream and application logs; confirm the runtime is .NET 8 and published files are at the ZIP root. For online search failures, confirm the token, restart after editing settings, and check outbound access to `api.themoviedb.org`. Cached search and saved collections remain available.

.NET 8 support ends November 10, 2026; plan a supported runtime upgrade before then. See the [official .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
