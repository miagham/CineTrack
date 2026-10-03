using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CineTrack.Controls;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>My Movies: everything the user has watched, with their ratings.</summary>
    public partial class MyMoviesPage : CinePage
    {
        readonly UniformGrid _stats = new() { Margin = new Thickness(0, 40, -16, 0) };
        readonly ComboBox _sort = new() { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        readonly UniformGrid _list = new() { Margin = new Thickness(0, 0, -16, 0), VerticalAlignment = VerticalAlignment.Top };
        readonly ContentControl _empty = new() { Focusable = false, IsTabStop = false };

        public MyMoviesPage()
        {
            InitializeComponent();
            Root.Children.Add(Components.PageHeader("Your history", "My Movies", "Every movie you've watched, with your ratings."));
            Root.Children.Add(_stats);

            var toolbar = new DockPanel { Margin = new Thickness(0, 16, 0, 20) };
            foreach (var s in new[] { "Recently watched", "Your rating", "Title (A–Z)" }) _sort.Items.Add(s);
            _sort.SelectedIndex = 0;
            System.Windows.Automation.AutomationProperties.SetName(_sort, "Sort watched movies");
            _sort.SelectionChanged += (_, _) => { if (IsLoaded) Render(); };
            toolbar.Children.Add(_sort);
            Root.Children.Add(toolbar);

            Root.Children.Add(_list);
            Root.Children.Add(_empty);
        }

        public override string? NavKey => "mymovies";

        protected override void Render()
        {
            var watched = Store.WatchedEntries.ToList();
            var rated = watched.Where(e => e.UserRating != null).ToList();

            _stats.Children.Clear();
            _stats.Children.Add(Components.StatCard(Glyphs.Movies, "Movies watched", Store.MoviesWatched.ToString()));
            _stats.Children.Add(Components.StatCard(Glyphs.Clock, "Hours watched", $"{Store.HoursWatched} hrs"));
            _stats.Children.Add(Components.StatCard(Glyphs.StarFill, "Your average rating",
                rated.Count == 0 ? "—" : $"★ {Format.Rating(rated.Average(e => e.UserRating!.Value))}"));
            _stats.Children.Add(Components.StatCard(Glyphs.Tag, "Top genre", Store.TopGenre ?? "—"));

            IEnumerable<UserMovieEntry> sorted = _sort.SelectedIndex switch
            {
                1 => watched.OrderByDescending(e => e.UserRating ?? -1).ThenByDescending(e => e.WatchedAt),
                2 => watched.OrderBy(e => Store.Get(e.MovieId)?.Title),
                _ => watched.OrderByDescending(e => e.WatchedAt),
            };

            _list.Children.Clear();
            foreach (var e in sorted)
            {
                var m = Store.Get(e.MovieId);
                if (m != null) _list.Children.Add(HistoryCard(m, e));
            }

            _empty.Content = _list.Children.Count > 0 ? null : Components.EmptyState(Glyphs.Movies, "Nothing watched yet",
                "When you mark a movie as watched, it lands here with your rating.", "Find something to watch", () => Shell.Discover());
        }

        Border HistoryCard(Movie m, UserMovieEntry e)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var poster = Components.Poster(m, 96, 144, 10);
            poster.VerticalAlignment = VerticalAlignment.Top;
            Components.MakeClickable(poster, $"Open details for {m.Title}", () => Shell.Details(m));
            grid.Children.Add(poster);

            var body = new StackPanel { Margin = new Thickness(18, 0, 0, 0) };
            Grid.SetColumn(body, 1);
            grid.Children.Add(body);

            var title = Components.Text(m.Title, "Text.CardTitle");
            title.FontSize = 20;
            title.TextWrapping = TextWrapping.Wrap;
            title.TextTrimming = TextTrimming.None;
            body.Children.Add(title);
            body.Children.Add(Components.Text(Format.Meta(m, withRuntime: true), "Text.Meta", new Thickness(0, 4, 0, 0)));
            body.Children.Add(Components.Text($"Watched {Format.RelativeTime(e.WatchedAt ?? e.AddedAt)}", "Text.Meta", new Thickness(0, 4, 0, 14)));

            var stars = new StarRating(22) { Value = e.UserRating ?? 0, Tag = $"rate:{m.Id}" };
            System.Windows.Automation.AutomationProperties.SetName(stars, $"Your rating for {m.Title}");
            stars.ValueChanged += (_, v) => Store.SetRating(m.Id, v);
            var ratingRow = new WrapPanel();
            ratingRow.Children.Add(stars);
            var label = Components.Text(e.UserRating is double r ? $"{Format.Rating(r)} your rating" : "Not rated yet", "Text.Meta", new Thickness(4, 0, 0, 0));
            label.VerticalAlignment = VerticalAlignment.Center;
            ratingRow.Children.Add(label);
            body.Children.Add(ratingRow);

            var trash = Components.IconButton(Glyphs.Delete, $"Remove {m.Title} from your history", () => MovieActions.RemoveFromHistory(m), small: true);
            trash.HorizontalAlignment = HorizontalAlignment.Left;
            trash.Margin = new Thickness(0, 14, 0, 0);
            body.Children.Add(trash);

            return new Border
            {
                Style = Ui.Res<Style>("Panel"),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 16, 16),
                Child = grid,
            };
        }

        protected override void ApplyLayout(Breakpoint bp)
        {
            _stats.Columns = bp == Breakpoint.Desktop ? 4 : bp == Breakpoint.Mobile ? 1 : 2;
            _list.Columns = bp == Breakpoint.Mobile ? 1 : 2;
        }
    }
}
