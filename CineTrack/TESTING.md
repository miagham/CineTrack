# Verification results

Verified locally on October 10, 2026 after switching the website to TMDB data.

- Release build: successful with no warnings or errors. Release publish: successful.
- Regression runner: 33 passing checks covering an empty initial catalog, cached search, collection actions, ratings, persistence serialization, collection isolation, and retention of full details during catalog refreshes.
- API regression checks cover trending, genre metadata, highly rated discovery, title/person/year/runtime search, image URLs, full details, query escaping, authentication headers, HTTP failures and missing configuration.
- Headless Chrome: 25 passing live checks. Discover and search poster images were verified to complete loading with nonzero image dimensions. Genre discovery, director search, runtime search, movie details, trailers, saving, watched history, ratings, reload persistence, removal, statistics, empty states and mobile layout were verified.
- The existing TMDB server token was used for live checks without writing its value to source files.

The production app contains no seed catalog. Small fixture movies exist only in the regression test project, allowing deterministic tests without network access.

Run commands and prerequisites are documented in [README.md](README.md). Browser tests use a separate profile and clear its collection before running. Azure deployment was not performed; follow [DEPLOY_AZURE.md](DEPLOY_AZURE.md) and its post-deployment checks.
