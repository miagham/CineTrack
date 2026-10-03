using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using CineTrack.Controls;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>Discover / Browse by Genre.</summary>
    public partial class GenresPage : CinePage
    {
        static readonly int[] YearOptions = { 1990, 1995, 2000, 2005, 2010, 2015, 2018, 2020, 2022, 2024, 2026 };

        string _selectedGenre;
        DiscoverFilters _applied = new();
        bool _built;

        public GenresPage() : this(null) { }

        public GenresPage(string? genre)
        {
            InitializeComponent();
            _selectedGenre = genre != null && GenreList.All.Contains(genre) ? genre : "Action";

            HeaderHost.Content = Components.PageHeader("Discover", "Browse by Genre",
                "Pick a genre, narrow it down, and add what catches your eye.");

            foreach (var box in new[] { YearFrom, YearTo })
            {
                box.Items.Add("Any");
                foreach (var y in YearOptions) box.Items.Add(y.ToString());
                box.SelectedIndex = 0;
            }

            foreach (var g in GenreList.All)
            {
                var chip = new Button { Content = g, Style = Ui.Res<Style>("Chip"), Tag = $"genre:{g}" };
                chip.Click += (_, _) => SelectGenre(g);
                GenreChips.Children.Add(chip);
            }
            UpdateChips();
        }

        public override string? NavKey => "discover";

        protected override void Render()
        {
            // Poster cards update their own tags; rows are only rebuilt when the genre or filters change.
            if (_built) return;
            _built = true;
            RenderRows();
        }

        void SelectGenre(string genre)
        {
            _selectedGenre = genre;
            UpdateChips();
            RenderRows();
        }

        void UpdateChips()
        {
            foreach (Button chip in GenreChips.Children)
            {
                var active = (string)chip.Content == _selectedGenre;
                Ui.SetIsActive(chip, active);
                AutomationProperties.SetName(chip, active ? $"{chip.Content}, selected" : $"Show {chip.Content} movies");
            }
        }

        void RenderRows()
        {
            Rows.Children.Clear();
            var matches = Store.Catalog.Where(_applied.Matches).ToList();

            // Selected genre first, then the other genres that still have matches, most titles first.
            var others = GenreList.All
                .Where(g => g != _selectedGenre)
                .Select(g => (Genre: g, Movies: matches.Where(m => m.Genres.Contains(g)).ToList()))
                .Where(x => x.Movies.Count > 0)
                .OrderByDescending(x => Store.Catalog.Count(m => m.Genres.Contains(x.Genre)))
                .ToList();

            Rows.Children.Add(GenreRow(_selectedGenre, matches.Where(m => m.Genres.Contains(_selectedGenre)).ToList(), selected: true));
            foreach (var (genre, movies) in others) Rows.Children.Add(GenreRow(genre, movies, selected: false));
        }

        FrameworkElement GenreRow(string genre, List<Movie> movies, bool selected)
        {
            var panel = new StackPanel();
            var header = new DockPanel { Margin = new Thickness(0, 0, 0, 16) };

            var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(right, Dock.Right);
            header.Children.Add(right);

            var titleRow = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
            var title = Components.Text($"{genre} Movies", "Text.Panel", new Thickness(0, 0, 12, 0));
            title.VerticalAlignment = VerticalAlignment.Center;
            titleRow.Children.Add(title);
            titleRow.Children.Add(Components.Tag(selected ? "SELECTED" : "POPULAR", Components.TagKind.Badge));
            header.Children.Add(titleRow);
            panel.Children.Add(header);

            if (movies.Count == 0)
            {
                var none = Components.Text(_applied.IsEmpty
                        ? $"No {genre} movies in the catalog yet."
                        : $"No {genre} movies match these filters. Try widening them or press Reset.",
                    "Text.Body");
                none.FontSize = 15;
                none.Foreground = Ui.Brush("TextMuted");
                panel.Children.Add(none);
            }
            else
            {
                var row = new PosterRow();
                row.SetMovies(movies.OrderByDescending(m => m.CommunityRating), Responsive.IsMobile ? 150 : 160);
                right.Children.Add(Components.LinkButton("See all", () => Shell.Search(genre), $"See all {genre} movies"));
                if (!Responsive.IsMobile) right.Children.Add(row.CreateArrows($"{genre} movies"));
                panel.Children.Add(row);
            }

            return new Border
            {
                Style = Ui.Res<Style>("Panel"),
                Padding = new Thickness(Responsive.IsMobile ? 16 : 24),
                Margin = new Thickness(0, 0, 0, 24),
                Child = panel,
            };
        }

        // ---------- Filters ----------

        DiscoverFilters ReadFilters() => new()
        {
            Runtime = new[] { RtUnder90.IsChecked == true, Rt90To120.IsChecked == true, Rt120To180.IsChecked == true, RtOver180.IsChecked == true },
            YearFrom = YearFrom.SelectedIndex > 0 ? int.Parse((string)YearFrom.SelectedItem) : null,
            YearTo = YearTo.SelectedIndex > 0 ? int.Parse((string)YearTo.SelectedItem) : null,
            Rating = new[] { Rating1To3.IsChecked == true, Rating3To4.IsChecked == true, Rating4Plus.IsChecked == true },
            NotWatched = NotWatched.IsChecked == true,
            HideWatchlist = HideWatchlist.IsChecked == true,
        };

        void Apply_Click(object sender, RoutedEventArgs e)
        {
            _applied = ReadFilters();
            UpdateFiltersToggle();
            Shell.CloseDrawer();
            RenderRows();
            var count = _applied.ActiveCount;
            Shell.Toast(count == 0 ? "Showing all movies" : $"{Format.Plural(count, "filter", "filters")} applied");
        }

        void Reset_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cb in new[] { RtUnder90, Rt90To120, Rt120To180, RtOver180, Rating1To3, Rating3To4, Rating4Plus, NotWatched, HideWatchlist })
                cb.IsChecked = false;
            YearFrom.SelectedIndex = YearTo.SelectedIndex = 0;
            _applied = new DiscoverFilters();
            UpdateFiltersToggle();
            RenderRows();
        }

        void UpdateFiltersToggle()
        {
            var n = _applied.ActiveCount;
            FiltersToggle.Content = n == 0 ? "Filters" : $"Filters ({n})";
            AutomationProperties.SetName(FiltersToggle, n == 0 ? "Open filters" : $"Open filters, {n} applied");
        }

        void FiltersToggle_Click(object sender, RoutedEventArgs e)
        {
            FiltersHost.Content = null;
            SetFiltersChrome(inDrawer: true);
            Shell.OpenDrawer(FiltersPanel, "Filters", onClosed: () =>
            {
                // Put the panel back so it is ready if the window grows to desktop width.
                if (FiltersHost.Content == null && Responsive.IsDesktop)
                {
                    SetFiltersChrome(inDrawer: false);
                    FiltersHost.Content = FiltersPanel;
                }
            });
        }

        void SetFiltersChrome(bool inDrawer)
        {
            FiltersPanel.Background = inDrawer ? System.Windows.Media.Brushes.Transparent : Ui.Brush("NearBlack");
            FiltersPanel.BorderThickness = new Thickness(inDrawer ? 0 : 1);
            FiltersPanel.Padding = new Thickness(inDrawer ? 0 : 22);
            FiltersHeading.Visibility = inDrawer ? Visibility.Collapsed : Visibility.Visible;
        }

        protected override void ApplyLayout(Breakpoint bp)
        {
            var desktop = bp == Breakpoint.Desktop;
            FiltersToggle.Visibility = desktop ? Visibility.Collapsed : Visibility.Visible;
            FiltersColumn.Width = new GridLength(desktop ? 240 : 0);
            Rows.Margin = new Thickness(desktop ? 32 : 0, 0, 0, 0);

            if (desktop)
            {
                Shell.CloseDrawer();
                if (FiltersHost.Content == null)
                {
                    SetFiltersChrome(inDrawer: false);
                    FiltersHost.Content = FiltersPanel;
                }
            }
            else if (FiltersHost.Content != null)
            {
                FiltersHost.Content = null;
            }

            if (_built) RenderRows();
        }
    }

    /// <summary>Discover filter state. Groups are OR'd inside and AND'd together.</summary>
    public sealed class DiscoverFilters
    {
        public bool[] Runtime { get; init; } = new bool[4];   // <90, 90–120, 120–180, >180
        public int? YearFrom { get; init; }
        public int? YearTo { get; init; }
        public bool[] Rating { get; init; } = new bool[3];    // 1–3, 3–4, 4+
        public bool NotWatched { get; init; }
        public bool HideWatchlist { get; init; }

        public int ActiveCount =>
            Runtime.Count(x => x) + Rating.Count(x => x) + (YearFrom != null ? 1 : 0) + (YearTo != null ? 1 : 0)
            + (NotWatched ? 1 : 0) + (HideWatchlist ? 1 : 0);

        public bool IsEmpty => ActiveCount == 0;

        public bool Matches(Movie m)
        {
            if (Runtime.Any(x => x))
            {
                var r = m.RuntimeMin;
                var ok = (Runtime[0] && r < 90) || (Runtime[1] && r >= 90 && r <= 120)
                      || (Runtime[2] && r > 120 && r <= 180) || (Runtime[3] && r > 180);
                if (!ok) return false;
            }
            if (YearFrom is int from && m.Year < from) return false;
            if (YearTo is int to && m.Year > to) return false;
            if (Rating.Any(x => x))
            {
                var s = m.CommunityRating;
                var ok = (Rating[0] && s >= 1 && s < 3) || (Rating[1] && s >= 3 && s < 4) || (Rating[2] && s >= 4);
                if (!ok) return false;
            }
            var entry = MovieStore.Instance.Entry(m.Id);
            if (NotWatched && entry?.IsWatched == true) return false;
            if (HideWatchlist && entry?.OnWatchlist == true) return false;
            return true;
        }
    }
}
