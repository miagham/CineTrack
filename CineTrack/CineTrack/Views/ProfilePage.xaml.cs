using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CineTrack.Controls;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>Profile: full stats plus local-data controls ("Full stats ›" from Home lands here).</summary>
    public partial class ProfilePage : CinePage
    {
        readonly UniformGrid _stats = new() { Margin = new Thickness(0, 40, -16, 0) };
        readonly Grid _columns = new() { Margin = new Thickness(0, 16, 0, 0) };
        readonly StackPanel _genreBars = new();
        readonly Border _dataPanel;

        public ProfilePage()
        {
            InitializeComponent();
            Root.Children.Add(Components.PageHeader("Profile", "Your Movie Stats",
                "A look at your viewing habits. Everything here is stored only on this device."));
            Root.Children.Add(_stats);

            _columns.ColumnDefinitions.Add(new ColumnDefinition());
            _columns.ColumnDefinitions.Add(new ColumnDefinition());
            _columns.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _columns.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var genres = new StackPanel();
            genres.Children.Add(Components.Text("What you watch", "Text.Panel", new Thickness(0, 0, 0, 6)));
            genres.Children.Add(Components.Text("Watched movies by genre", "Text.Meta", new Thickness(0, 0, 0, 20)));
            genres.Children.Add(_genreBars);
            _columns.Children.Add(new Border { Style = Ui.Res<Style>("Panel"), Padding = new Thickness(28), VerticalAlignment = VerticalAlignment.Top, Child = genres });

            var data = new StackPanel();
            data.Children.Add(Components.Text("Your data", "Text.Panel", new Thickness(0, 0, 0, 12)));
            var info = Components.Text("CineTrack has no accounts. Your watchlist, ratings and history are saved in a file on this computer:", "Text.Body");
            info.FontSize = 15;
            info.LineHeight = 23;
            data.Children.Add(info);
            var path = Components.Text(Store.DataPath, "Text.Meta", new Thickness(0, 10, 0, 22));
            path.TextWrapping = TextWrapping.Wrap;
            path.TextTrimming = TextTrimming.None;
            data.Children.Add(path);

            var buttons = new WrapPanel();
            var reset = Components.ActionButton("Reset to sample data", "Btn.Secondary", null, () => Replace(Store.ResetToSampleData, "Sample data restored"));
            reset.Margin = new Thickness(0, 0, 10, 10);
            var clear = Components.ActionButton("Clear all data", "Btn.Tertiary", Glyphs.Delete, () => Replace(Store.ClearAll, "All your data was cleared"));
            clear.Margin = new Thickness(0, 0, 0, 10);
            buttons.Children.Add(reset);
            buttons.Children.Add(clear);
            data.Children.Add(buttons);

            _dataPanel = new Border { Style = Ui.Res<Style>("Panel"), Padding = new Thickness(28), VerticalAlignment = VerticalAlignment.Top, Child = data };
            Grid.SetColumn(_dataPanel, 1);
            _columns.Children.Add(_dataPanel);
            Root.Children.Add(_columns);
        }

        public override string? NavKey => "profile";

        void Replace(Action change, string message)
        {
            var snapshot = Store.SnapshotAll();
            change();
            Shell.Toast(message, () => Store.RestoreAll(snapshot));
        }

        protected override void Render()
        {
            var rated = Store.WatchedEntries.Where(e => e.UserRating != null).ToList();
            _stats.Children.Clear();
            _stats.Children.Add(Components.StatCard(Glyphs.Tag, "Top genre", Store.TopGenre ?? "—"));
            _stats.Children.Add(Components.StatCard(Glyphs.Clock, "Hours watched", $"{Store.HoursWatched} hrs"));
            _stats.Children.Add(Components.StatCard(Glyphs.Movies, "Movies watched", Store.MoviesWatched.ToString()));
            _stats.Children.Add(Components.StatCard(Glyphs.Bookmark, "Want to watch", Store.WantToWatchCount.ToString()));
            _stats.Children.Add(Components.StatCard(Glyphs.Library, "Total saved", Store.TotalSaved.ToString()));
            _stats.Children.Add(Components.StatCard(Glyphs.StarFill, "High priority", Store.HighPriorityCount.ToString()));
            _stats.Children.Add(Components.StatCard(Glyphs.Star, "Your average rating",
                rated.Count == 0 ? "—" : $"★ {Format.Rating(rated.Average(e => e.UserRating!.Value))}"));
            _stats.Children.Add(Components.StatCard(Glyphs.Clock, "Time to clear your list", Format.Duration(Store.TimeToClearMinutes)));

            _genreBars.Children.Clear();
            var counts = Store.WatchedEntries
                .Select(e => Store.Get(e.MovieId)?.PrimaryGenre)
                .Where(g => g != null)
                .GroupBy(g => g!)
                .Select(g => (Genre: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Count).ThenBy(x => x.Genre)
                .ToList();
            var max = counts.Count > 0 ? counts.Max(x => x.Count) : 1;
            foreach (var (genre, count) in counts)
                _genreBars.Children.Add(Components.ProgressRow(genre, count, (double)count / max));
            if (counts.Count == 0)
                _genreBars.Children.Add(Components.Text("Watch something to see your genre mix.", "Text.Meta"));
        }

        protected override void ApplyLayout(Breakpoint bp)
        {
            var desktop = bp == Breakpoint.Desktop;
            _stats.Columns = desktop ? 4 : bp == Breakpoint.Mobile ? 1 : 2;
            _columns.ColumnDefinitions[1].Width = desktop ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            Grid.SetColumn(_dataPanel, desktop ? 1 : 0);
            Grid.SetRow(_dataPanel, desktop ? 0 : 1);
            _dataPanel.Margin = desktop ? new Thickness(24, 0, 0, 0) : new Thickness(0, 24, 0, 0);
        }
    }
}
