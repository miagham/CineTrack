using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CineTrack.Controls;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>Home (spec 6.1): hero, search, stats, trending, recent history and watchlist.</summary>
    public partial class HomePage : CinePage
    {
        readonly List<Movie> _heroMovies;
        readonly DispatcherTimer _heroTimer = new() { Interval = TimeSpan.FromSeconds(8) };
        int _heroIndex;

        public HomePage()
        {
            InitializeComponent();

            _heroMovies = Store.Recommendations(4);
            _heroTimer.Tick += (_, _) =>
            {
                if (Hero.IsMouseOver || Hero.IsKeyboardFocusWithin) return;   // pause on hover/focus
                ShowHero((_heroIndex + 1) % _heroMovies.Count, animate: true);
            };
            Loaded += (_, _) => { if (!Ui.ReducedMotion && _heroMovies.Count > 1) _heroTimer.Start(); };
            Unloaded += (_, _) => _heroTimer.Stop();

            HomeSearch.SetLarge(true, showFilters: true);
            SuggestionsHost.Content = Components.SuggestionChips(q => Shell.Search(q));

            StatsHeader.Content = Components.SectionHeader("Your year in film", "Your Movie Stats",
                Components.LinkButton("Full stats", Shell.Profile));

            var trendingActions = new StackPanel { Orientation = Orientation.Horizontal };
            trendingActions.Children.Add(Components.LinkButton("See all", () => Shell.Discover(), "See all movies"));
            trendingActions.Children.Add(TrendingRow.CreateArrows("trending movies"));
            TrendingHeader.Content = Components.SectionHeader("Now showing", "Trending Movies", trendingActions);
            TrendingRow.SetMovies(SeedData.TrendingIds.Select(Store.Get).OfType<Movie>(), 180);

            HistoryHeader.Content = Components.SectionHeader("", "Recent History",
                Components.LinkButton("View all", Shell.MyMovies, "View all watched movies"), "Text.Panel");
            WatchlistHeader.Content = Components.SectionHeader("", "My Watchlist",
                Components.LinkButton("View all", Shell.Watchlist, "View whole watchlist"), "Text.Panel");

            ShowHero(0, animate: false);
        }

        public override string? NavKey => "home";

        public void FocusSearch()
        {
            HomeSearch.BringIntoView();
            HomeSearch.FocusInput();
        }

        // ---------- Hero ----------

        void ShowHero(int index, bool animate)
        {
            if (_heroMovies.Count == 0) return;
            _heroIndex = index;
            var m = _heroMovies[index];

            HeroBackdrop.Content = Components.PosterArt(m, showTitle: false);
            HeroTitle.Text = m.Title;
            HeroSynopsis.Text = m.Synopsis;

            HeroMeta.Children.Clear();
            void MetaText(string text)
            {
                var t = Components.Text(text, "Text.Meta", new Thickness(0, 0, 0, 6));
                t.FontSize = 15;
                t.Foreground = Ui.Brush("Cream");
                t.VerticalAlignment = VerticalAlignment.Center;
                HeroMeta.Children.Add(t);
            }
            MetaText($"{m.Year}{Format.Dot}{string.Join(" / ", m.Genres.Take(2))}{Format.Dot}{Format.Runtime(m.RuntimeMin)}{Format.Dot}");
            if (m.Certification != null)
            {
                HeroMeta.Children.Add(new Border
                {
                    BorderBrush = Ui.Brush("TextMuted"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 1, 6, 1),
                    Margin = new Thickness(0, 0, 0, 6),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = Components.Text(m.Certification, "Text.Meta"),
                });
                MetaText(Format.Dot);
            }
            var rating = Components.RatingLabel(m.CommunityRating, size: 15);
            rating.Margin = new Thickness(0, 0, 0, 6);
            HeroMeta.Children.Add(rating);

            RenderHeroButtons();
            RenderHeroDots();

            if (animate && !Ui.ReducedMotion)
            {
                HeroText.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(450)));
                HeroBackdrop.BeginAnimation(OpacityProperty, new DoubleAnimation(0.2, 1, TimeSpan.FromMilliseconds(600)));
            }
        }

        void RenderHeroButtons()
        {
            if (_heroMovies.Count == 0) return;
            var m = _heroMovies[_heroIndex];
            var entry = Store.Entry(m.Id);
            HeroButtons.Children.Clear();

            Button Add(Button b)
            {
                b.Margin = new Thickness(0, 0, 12, 12);
                HeroButtons.Children.Add(b);
                return b;
            }

            if (entry?.OnWatchlist == true)
                Add(Components.ActionButton("On watchlist", "Btn.Done", Glyphs.Check, Shell.Watchlist, $"{m.Title} is on your watchlist. Open watchlist"));
            else
                Add(Components.ActionButton("Add to Watchlist", "Btn.Primary", Glyphs.Add, () => MovieActions.AddToWatchlist(m), $"Add {m.Title} to watchlist"));

            Add(Components.ActionButton("Watch Trailer", "Btn.Secondary", Glyphs.Play, () => MovieActions.Trailer(m), $"Watch the {m.Title} trailer"));

            if (entry?.IsWatched == true)
                Add(Components.ActionButton("Watched", "Btn.Done", Glyphs.Check, () => MovieActions.ToggleWatched(m), $"{m.Title} is watched. Mark as unwatched"));
            else
                Add(Components.ActionButton("Already watched", "Btn.Tertiary", Glyphs.Check, () => MovieActions.ToggleWatched(m), $"I've already watched {m.Title}"));

            foreach (FrameworkElement b in HeroButtons.Children) b.Tag = $"hero:{HeroButtons.Children.IndexOf(b)}:{m.Id}";
        }

        void RenderHeroDots()
        {
            HeroDots.Children.Clear();
            if (_heroMovies.Count < 2) return;
            for (var i = 0; i < _heroMovies.Count; i++)
            {
                var index = i;
                var active = i == _heroIndex;
                var dot = new Button
                {
                    Style = Ui.Res<Style>("Btn.Base"),
                    Padding = new Thickness(0),
                    Width = active ? 28 : 10,
                    Height = 10,
                    Margin = new Thickness(6, 0, 0, 0),
                    Background = Ui.Brush(active ? "Gold" : "TextMuted"),
                    BorderThickness = new Thickness(0),
                    Tag = $"dot:{i}",
                };
                AutomationProperties.SetName(dot, $"Show recommendation {i + 1} of {_heroMovies.Count}: {_heroMovies[i].Title}");
                dot.Click += (_, _) =>
                {
                    ShowHero(index, animate: true);
                    _heroTimer.Stop();
                    if (!Ui.ReducedMotion) _heroTimer.Start();
                };
                HeroDots.Children.Add(dot);
            }
        }

        // ---------- Data-driven sections ----------

        protected override void Render()
        {
            RenderHeroButtons();

            StatsGrid.Children.Clear();
            StatsGrid.Children.Add(Components.StatCard(Glyphs.Tag, "Top genre", Store.TopGenre ?? "—"));
            StatsGrid.Children.Add(Components.StatCard(Glyphs.Clock, "Hours watched", $"{Store.HoursWatched} hrs"));
            StatsGrid.Children.Add(Components.StatCard(Glyphs.Movies, "Movies watched", Store.MoviesWatched.ToString()));
            StatsGrid.Children.Add(Components.StatCard(Glyphs.Bookmark, "Want to watch", Store.WantToWatchCount.ToString()));

            RenderHistory();
            RenderWatchlist();
        }

        void RenderHistory()
        {
            var history = Store.RecentHistory(3);
            if (history.Count == 0)
            {
                HistoryHost.Content = Components.EmptyState(Glyphs.Movies, "Nothing watched yet",
                    "Mark a movie as watched and it will show up here with your rating.", "Find something to watch", () => Shell.Discover());
                return;
            }

            var grid = new UniformGrid { Columns = Responsive.IsMobile ? 1 : 3, Margin = new Thickness(0, 0, -20, 0) };
            foreach (var e in history)
            {
                var m = Store.Get(e.MovieId);
                if (m == null) continue;
                var item = new Grid { Margin = new Thickness(0, 0, 20, 12), Background = System.Windows.Media.Brushes.Transparent };
                item.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                item.ColumnDefinitions.Add(new ColumnDefinition());
                var poster = Components.Poster(m, 76, 114, 8);
                poster.VerticalAlignment = VerticalAlignment.Top;
                item.Children.Add(poster);

                var text = new StackPanel { Margin = new Thickness(14, 2, 0, 0) };
                var title = Components.Text(m.Title, "Text.CardTitle");
                title.TextWrapping = TextWrapping.Wrap;
                title.TextTrimming = TextTrimming.None;
                text.Children.Add(title);
                text.Children.Add(Components.Text(Format.Meta(m), "Text.Meta", new Thickness(0, 4, 0, 8)));
                if (e.UserRating is double r)
                {
                    text.Children.Add(Components.RatingLabel(r, "your rating"));
                }
                else
                {
                    var rate = Components.LinkButton("Rate it", () => Shell.PromptRating(m), $"Rate {m.Title}");
                    rate.HorizontalAlignment = HorizontalAlignment.Left;
                    rate.Padding = new Thickness(0, 2, 0, 2);
                    text.Children.Add(rate);
                }
                var when = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 0) };
                var clock = Components.Icon(Glyphs.Clock, 12, Ui.Brush("TextMuted"));
                clock.Margin = new Thickness(0, 0, 6, 0);
                when.Children.Add(clock);
                when.Children.Add(Components.Text(Format.RelativeTime(e.WatchedAt ?? e.AddedAt), "Text.Meta"));
                text.Children.Add(when);

                Grid.SetColumn(text, 1);
                item.Children.Add(text);
                Components.MakeClickable(item, $"{m.Title}, watched {Format.RelativeTime(e.WatchedAt ?? e.AddedAt)}. Open details", () => Shell.Details(m));
                grid.Children.Add(item);
            }
            HistoryHost.Content = grid;
        }

        void RenderWatchlist()
        {
            WatchlistItems.Children.Clear();
            var items = Store.WatchlistDefaultOrder().Where(e => !e.IsWatched).Take(3).ToList();
            if (items.Count == 0)
            {
                WatchlistItems.Children.Add(Components.EmptyState(Glyphs.Bookmark, "Your watchlist is empty",
                    "Save movies you want to see next.", "Find movies to add", () => Shell.Discover()));
                return;
            }
            foreach (var e in items)
            {
                var m = Store.Get(e.MovieId);
                if (m != null) WatchlistItems.Children.Add(Components.CompactRow(m, showPriorityDetails: false));
            }
        }

        // ---------- Responsive ----------

        protected override void ApplyLayout(Breakpoint bp)
        {
            var mobile = bp == Breakpoint.Mobile;
            var desktop = bp == Breakpoint.Desktop;

            HeroTitle.FontSize = desktop ? 92 : mobile ? 48 : 68;
            HeroTitle.LineHeight = HeroTitle.FontSize * 1.05;
            HeroInner.Margin = mobile ? new Thickness(24, 32, 24, 28) : new Thickness(56, 56, 56, 44);
            HeroBackdrop.Width = double.NaN;
            HeroBackdrop.Margin = new Thickness(mobile ? 0 : desktop ? 440 : 280, 0, 0, 0);
            HeroFade.Opacity = mobile ? 0.92 : 1;
            Hero.MinHeight = mobile ? 0 : 500;
            HeroSynopsis.FontSize = mobile ? 16 : 18;
            HeroSynopsis.LineHeight = mobile ? 26 : 29;
            HeroDots.Margin = mobile ? new Thickness(0, 24, 0, 0) : new Thickness(0);
            HeroDots.VerticalAlignment = VerticalAlignment.Bottom;
            HeroText.Margin = mobile ? new Thickness(0, 0, 0, 36) : new Thickness(0);

            StatsGrid.Columns = desktop ? 4 : mobile ? 1 : 2;

            // Bottom row: side by side on desktop, stacked otherwise.
            HistoryColumn.Width = desktop ? new GridLength(2, GridUnitType.Star) : new GridLength(1, GridUnitType.Star);
            WatchlistColumn.Width = desktop ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            Grid.SetColumn(WatchlistPanel, desktop ? 1 : 0);
            Grid.SetRow(WatchlistPanel, desktop ? 0 : 1);
            WatchlistPanel.Margin = desktop ? new Thickness(24, 0, 0, 0) : new Thickness(0, 24, 0, 0);
            HistoryPanel.Padding = WatchlistPanel.Padding = new Thickness(mobile ? 20 : 28);

            if (IsLoaded) RenderHistory();
        }
    }
}
