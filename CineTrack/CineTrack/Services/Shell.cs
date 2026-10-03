using System.Diagnostics;
using System.Windows;
using CineTrack.Models;

namespace CineTrack.Services
{
    /// <summary>App-wide navigation, toasts, and the rating prompt. Implemented by MainWindow.</summary>
    public static class Shell
    {
        static MainWindow Window => (MainWindow)Application.Current.MainWindow;

        public static void Home() => Window.Navigate(new HomePage());
        public static void Discover(string? genre = null) => Window.Navigate(new GenresPage(genre));
        public static void Watchlist() => Window.Navigate(new WatchlistPage());
        public static void MyMovies() => Window.Navigate(new MyMoviesPage());
        public static void Details(Movie movie) => Window.Navigate(new MovieDetailsPage(movie));
        public static void Search(string query) => Window.Navigate(new SearchPage(query));
        public static void Profile() => Window.Navigate(new ProfilePage());
        public static void About() => Window.Navigate(new AboutPage());

        public static void Toast(string message, Action? undo = null) => Window.ShowToast(message, undo);

        public static void PromptRating(Movie movie) => Window.ShowRatingPrompt(movie);

        public static void OpenDrawer(FrameworkElement content, string title, Action? onClosed = null) =>
            Window.OpenDrawer(content, title, onClosed);

        public static void CloseDrawer() => Window.CloseDrawer();

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception)
            {
                Toast("Couldn't open your browser.");
            }
        }
    }

    /// <summary>User actions shared by every screen.</summary>
    public static class MovieActions
    {
        static MovieStore Store => MovieStore.Instance;

        public static void AddToWatchlist(Movie m)
        {
            var before = Store.Entry(m.Id)?.Clone();
            Store.AddToWatchlist(m.Id);
            Shell.Toast($"Added {m.Title} to your watchlist", () =>
            {
                if (before == null) Store.RemoveFromWatchlist(m.Id);
                else Store.Restore(before);
            });
        }

        /// <summary>Marks watched and asks for a (skippable) rating. On a watched movie, undoes it instead.</summary>
        public static void ToggleWatched(Movie m)
        {
            var entry = Store.Entry(m.Id);
            if (entry?.IsWatched == true)
            {
                var snapshot = Store.MarkUnwatched(m.Id);
                Shell.Toast($"Marked {m.Title} as unwatched", snapshot == null ? null : () => Store.Restore(snapshot));
                return;
            }
            Store.MarkWatched(m.Id);
            Shell.PromptRating(m);
        }

        public static void TogglePriority(Movie m)
        {
            if (Store.Entry(m.Id)?.OnWatchlist != true) Store.AddToWatchlist(m.Id);
            var on = Store.TogglePriority(m.Id);
            Shell.Toast(on ? $"{m.Title} is now high priority" : $"{m.Title} is no longer high priority");
        }

        public static void RemoveFromWatchlist(Movie m)
        {
            var snapshot = Store.RemoveFromWatchlist(m.Id);
            if (snapshot == null) return;
            Shell.Toast($"Removed {m.Title} from your watchlist", () => Store.Restore(snapshot));
        }

        public static void RemoveFromHistory(Movie m)
        {
            var snapshot = Store.MarkUnwatched(m.Id);
            if (snapshot == null) return;
            Shell.Toast($"Removed {m.Title} from your history", () => Store.Restore(snapshot));
        }

        public static void Trailer(Movie m) => Shell.OpenUrl(m.TrailerUrl
            ?? "https://www.youtube.com/results?search_query=" + Uri.EscapeDataString($"{m.Title} {m.Year} official trailer"));
    }
}
