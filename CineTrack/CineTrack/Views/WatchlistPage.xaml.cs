using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using CineTrack.Controls;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>My Watchlist (spec 6.3).</summary>
    public partial class WatchlistPage : CinePage
    {
        enum Filter { All, Unwatched, Watched, HighPriority }

        static readonly string[] SortOptions = { "Recently added", "Title (A–Z)", "Shortest first", "Highest rated" };

        Filter _filter = Filter.All;
        bool _listView;

        public WatchlistPage()
        {
            InitializeComponent();
            HeaderHost.Content = Components.PageHeader("Your queue", "My Watchlist",
                "Everything you've saved, with your must-sees up front.");
            PicksHeader.Content = Components.SectionHeader("", "High Priority Picks",
                Components.LinkButton("View all", () => SetFilter(Filter.HighPriority), "View all high priority movies"), "Text.Panel");

            foreach (var s in SortOptions) SortBox.Items.Add(s);
            SortBox.SelectedIndex = 0;
        }

        public override string? NavKey => "watchlist";

        void FindMovies_Click(object sender, RoutedEventArgs e) => Shell.Discover();

        void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) RenderCards();
        }

        void GridView_Click(object sender, RoutedEventArgs e) { _listView = false; RenderCards(); }
        void ListView_Click(object sender, RoutedEventArgs e) { _listView = true; RenderCards(); }

        void SetFilter(Filter f)
        {
            _filter = f;
            RenderTabs();
            RenderCards();
        }

        protected override void Render()
        {
            StatsGrid.Children.Clear();
            StatsGrid.Children.Add(Components.StatCard(Glyphs.Library, "Total saved", Store.TotalSaved.ToString()));
            StatsGrid.Children.Add(Components.StatCard(Glyphs.StarFill, "High priority", Store.HighPriorityCount.ToString()));
            StatsGrid.Children.Add(Components.StatCard(Glyphs.Check, "Watched", Store.WatchlistWatchedCount.ToString()));
            StatsGrid.Children.Add(Components.StatCard(Glyphs.Eye, "Unwatched", Store.WatchlistUnwatchedCount.ToString()));

            RenderTabs();
            RenderCards();
            RenderSidebar();
        }

        void RenderTabs()
        {
            Tabs.Children.Clear();
            AddTab(Filter.All, "All", Store.TotalSaved);
            AddTab(Filter.Unwatched, "Unwatched", Store.WatchlistUnwatchedCount);
            AddTab(Filter.Watched, "Watched", Store.WatchlistWatchedCount);
            AddTab(Filter.HighPriority, "High priority", Store.HighPriorityCount);
        }

        void AddTab(Filter f, string label, int count)
        {
            var active = f == _filter;
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            content.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center });
            content.Children.Add(new PillBorder
            {
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(7, 1, 7, 1),
                Background = Ui.Brush(active ? "NearBlack" : "NearBlackMuted"),
                Child = new TextBlock
                {
                    Text = count.ToString(),
                    FontSize = 12,
                    FontWeight = FontWeights.ExtraBold,
                    Foreground = Ui.Brush(active ? "Gold" : "Cream"),
                },
            });
            var tab = new Button { Content = content, Style = Ui.Res<Style>("Chip"), Padding = new Thickness(16, 8, 10, 8), Tag = $"tab:{f}" };
            Ui.SetIsActive(tab, active);
            AutomationProperties.SetName(tab, $"{label}, {count}{(active ? ", selected" : "")}");
            tab.Click += (_, _) => SetFilter(f);
            Tabs.Children.Add(tab);
        }

        IEnumerable<UserMovieEntry> FilteredSorted()
        {
            var entries = Store.WatchlistDefaultOrder().Where(e => _filter switch
            {
                Filter.Unwatched => !e.IsWatched,
                Filter.Watched => e.IsWatched,
                Filter.HighPriority => e.HighPriority,
                _ => true,
            });
            Movie M(UserMovieEntry e) => Store.Get(e.MovieId)!;
            return SortBox.SelectedIndex switch
            {
                1 => entries.OrderBy(e => M(e).Title),
                2 => entries.OrderBy(e => M(e).RuntimeMin),
                3 => entries.OrderByDescending(e => M(e).CommunityRating),
                _ => entries,   // default: must-sees up front, then newest
            };
        }

        void RenderCards()
        {
            var compact = Responsive.IsMobile;
            var single = _listView || Responsive.IsMobile;
            CardsGrid.Columns = single ? 1 : 2;
            Ui.SetIsActive(GridViewButton, !_listView);
            Ui.SetIsActive(ListViewButton, _listView);

            CardsGrid.Children.Clear();
            foreach (var e in FilteredSorted())
            {
                var m = Store.Get(e.MovieId);
                if (m != null) CardsGrid.Children.Add(WatchlistCard.Create(m, e, compact));
            }

            if (CardsGrid.Children.Count > 0)
            {
                EmptyHost.Content = null;
            }
            else if (Store.TotalSaved == 0)
            {
                EmptyHost.Content = Components.EmptyState(Glyphs.Bookmark, "Your watchlist is empty",
                    "Save movies from Discover or Home and they'll line up here.", "Find movies to add", () => Shell.Discover());
            }
            else
            {
                EmptyHost.Content = Components.EmptyState(Glyphs.Filter, "Nothing here",
                    _filter switch
                    {
                        Filter.Watched => "You haven't watched anything on your list yet.",
                        Filter.Unwatched => "You've watched everything on your list. Time to find more!",
                        _ => "Star a movie on your watchlist to make it high priority.",
                    });
            }
        }

        void RenderSidebar()
        {
            PicksList.Children.Clear();
            var picks = Store.WatchlistDefaultOrder().Where(e => e.HighPriority && !e.IsWatched).Take(4).ToList();
            if (picks.Count == 0)
            {
                var none = Components.Text("Star a movie to pin it here.", "Text.Meta");
                none.TextWrapping = TextWrapping.Wrap;
                PicksList.Children.Add(none);
            }
            foreach (var e in picks)
            {
                var m = Store.Get(e.MovieId);
                if (m != null) PicksList.Children.Add(Components.CompactRow(m, showPriorityDetails: true));
            }

            GenreBars.Children.Clear();
            var counts = Store.WatchlistGenreCounts();
            var max = counts.Count > 0 ? counts.Max(c => c.Count) : 1;
            foreach (var (genre, count) in counts)
                GenreBars.Children.Add(Components.ProgressRow(genre, count, (double)count / max));
            if (counts.Count == 0)
                GenreBars.Children.Add(Components.Text("Add movies to see your genre mix.", "Text.Meta", new Thickness(0, 0, 0, 16)));

            TimeToClear.Text = Format.Duration(Store.TimeToClearMinutes);
        }

        protected override void ApplyLayout(Breakpoint bp)
        {
            var desktop = bp == Breakpoint.Desktop;
            var mobile = bp == Breakpoint.Mobile;

            // Sidebar moves below the main column on tablet and mobile.
            SidebarColumn.Width = desktop ? new GridLength(340) : new GridLength(0);
            Grid.SetColumn(Sidebar, desktop ? 1 : 0);
            Grid.SetRow(Sidebar, desktop ? 0 : 1);
            Sidebar.Margin = desktop ? new Thickness(32, 0, 0, 0) : new Thickness(0, 24, 0, 0);

            StatsGrid.Columns = desktop ? 4 : 2;

            // Header button and toolbar controls wrap under on mobile.
            Grid.SetColumn(FindMoviesButton, mobile ? 0 : 1);
            Grid.SetRow(FindMoviesButton, mobile ? 1 : 0);
            FindMoviesButton.HorizontalAlignment = mobile ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            FindMoviesButton.Margin = mobile ? new Thickness(0, 20, 0, 0) : new Thickness(24, 0, 0, 6);

            Grid.SetColumn(ToolbarRight, mobile ? 0 : 1);
            Grid.SetRow(ToolbarRight, mobile ? 1 : 0);
            ToolbarRight.Margin = mobile ? new Thickness(0, 4, 0, 0) : new Thickness(16, 0, 0, 10);
            ToolbarRight.HorizontalAlignment = HorizontalAlignment.Left;
            SortBox.Width = mobile ? 170 : 190;

            if (IsLoaded) RenderCards();
        }
    }
}
