using System.Windows;
using System.Windows.Controls;
using CineTrack.Controls;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>Search results: matches title, people, genre and phrases like "under 2 hours".</summary>
    public partial class SearchPage : CinePage
    {
        readonly SearchBar _bar = new();
        readonly TextBlock _heading;
        readonly TextBlock _count;
        readonly WrapPanel _results = new() { Margin = new Thickness(0, 8, -16, 0) };
        readonly ContentControl _empty = new() { Focusable = false, IsTabStop = false };
        string _query;

        public SearchPage() : this("") { }

        public SearchPage(string query)
        {
            InitializeComponent();
            _query = query;

            Root.Children.Add(Components.Text("SEARCH", "Text.Eyebrow", new Thickness(0, 0, 0, 10)));
            _heading = Components.Text("", "Text.PageTitle", new Thickness(0, 0, 0, 24));
            Root.Children.Add(_heading);

            _bar.SetLarge(true, showFilters: true);
            _bar.Text = query;
            _bar.Submitted += (_, q) => Run(q);
            Root.Children.Add(_bar);
            Root.Children.Add(Components.SuggestionChips(q => { _bar.Text = q; Run(q); }));

            _count = Components.Text("", "Text.Eyebrow", new Thickness(0, 48, 0, 16));
            Root.Children.Add(_count);
            Root.Children.Add(_results);
            Root.Children.Add(_empty);

            Loaded += (_, _) => { if (_query.Length == 0) FocusSearch(); };
        }

        public void FocusSearch() => _bar.FocusInput();

        void Run(string q)
        {
            _query = q.Trim();
            Render();
        }

        protected override void Render()
        {
            _results.Children.Clear();
            Title = _query.Length == 0 ? "Search" : $"Search: {_query}";
            _heading.Text = _query.Length == 0 ? "Find a movie" : $"Results for “{_query}”";

            if (_query.Length == 0)
            {
                _count.Visibility = Visibility.Collapsed;
                _empty.Content = Components.EmptyState(Glyphs.Search, "What are you in the mood for?",
                    "Search by title, director, actor, genre, or try a phrase like “Sci-Fi under 2 hours”.");
                return;
            }

            var results = MovieSearch.Run(_query);
            _count.Visibility = Visibility.Visible;
            _count.Text = Format.Plural(results.Count, "MATCH", "MATCHES");
            var width = Responsive.IsMobile ? 160 : 180;
            foreach (var m in results)
            {
                var card = new PosterCard(m, width) { Margin = new Thickness(0, 0, 16, 16) };
                _results.Children.Add(card);
            }
            _empty.Content = results.Count > 0 ? null : Components.EmptyState(Glyphs.Search, "No matches",
                $"Nothing in the catalog matches “{_query}”. Try a title, a person, or a genre.",
                "Browse by genre", () => Shell.Discover());
        }

        protected override void ApplyLayout(Breakpoint bp)
        {
            _heading.FontSize = bp == Breakpoint.Mobile ? 40 : 60;
            if (IsLoaded) Render();
        }
    }
}
