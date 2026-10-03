using System.Windows;
using System.Windows.Controls;
using CineTrack.Controls;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>Movie details: hero-style header, facts, the user's rating, and similar movies.</summary>
    public partial class MovieDetailsPage : CinePage
    {
        readonly Movie _movie;
        readonly Grid _hero = new();
        readonly Grid _heroInner = new();
        readonly TextBlock _title;
        readonly WrapPanel _tags = new() { Margin = new Thickness(0, 0, 0, 14) };
        readonly WrapPanel _buttons = new();
        readonly Grid _columns = new() { Margin = new Thickness(0, 32, 0, 0) };
        readonly Border _factsPanel;
        readonly Border _ratingPanel = new();
        readonly StackPanel _ratingBody = new();

        public MovieDetailsPage() : this(MovieStore.Instance.Catalog[0]) { }

        public MovieDetailsPage(Movie movie)
        {
            InitializeComponent();
            _movie = movie;
            Title = movie.Title;

            var back = Components.ActionButton("Back", "Btn.Link", Glyphs.Back, GoBack, "Go back");
            back.HorizontalAlignment = HorizontalAlignment.Left;
            back.Margin = new Thickness(-4, 0, 0, 16);
            Root.Children.Add(back);

            // Hero header, same treatment as Home.
            var heroBorder = new Border
            {
                Background = Ui.Brush("NearBlack"),
                BorderBrush = Ui.Brush("BorderSubtle"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(20),
                Child = _hero,
            };
            Ui.SetClipRadius(heroBorder, 20);
            _hero.Children.Add(new ContentControl { Content = Components.PosterArt(movie, showTitle: false), Focusable = false, IsTabStop = false });
            _hero.Children.Add(new Border { Background = Ui.Brush("HeroFadeBrush") });
            _hero.Children.Add(_heroInner);

            var text = new StackPanel { MaxWidth = 680, HorizontalAlignment = HorizontalAlignment.Left };
            text.Children.Add(Components.Text(string.Join("  ·  ", movie.Genres).ToUpperInvariant(), "Text.Eyebrow"));
            _title = Components.Text(movie.Title, "Text.Display", new Thickness(0, 14, 0, 16));
            text.Children.Add(_title);
            text.Children.Add(_tags);
            var meta = new WrapPanel { Margin = new Thickness(0, 0, 0, 18) };
            var metaText = Components.Text($"{movie.Year}{Format.Dot}{Format.Runtime(movie.RuntimeMin)}{Format.Dot}{movie.Certification ?? "Not rated"}{Format.Dot}", "Text.Meta");
            metaText.FontSize = 15;
            metaText.Foreground = Ui.Brush("Cream");
            meta.Children.Add(metaText);
            meta.Children.Add(Components.RatingLabel(movie.CommunityRating, "community", 15));
            text.Children.Add(meta);
            var synopsis = Components.Text(movie.Synopsis, "Text.Body", new Thickness(0, 0, 0, 30));
            synopsis.MaxWidth = 520;
            synopsis.HorizontalAlignment = HorizontalAlignment.Left;
            text.Children.Add(synopsis);
            text.Children.Add(_buttons);
            _heroInner.Children.Add(text);
            Root.Children.Add(heroBorder);

            // Facts + your rating.
            _columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            _columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _columns.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _columns.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var facts = new StackPanel();
            facts.Children.Add(Components.Text("About this movie", "Text.Panel", new Thickness(0, 0, 0, 20)));
            AddFact(facts, "Directed & starring", string.Join(", ", movie.People));
            AddFact(facts, "Genres", string.Join(" / ", movie.Genres));
            AddFact(facts, "Runtime", Format.Runtime(movie.RuntimeMin));
            AddFact(facts, "Released", movie.Year.ToString());
            if (movie.Certification != null) AddFact(facts, "Rated", movie.Certification);
            AddFact(facts, "Community rating", $"★ {Format.Rating(movie.CommunityRating)} out of 5");
            if (movie.Keywords.Count > 0) AddFact(facts, "Notes", string.Join(" · ", movie.Keywords));
            _factsPanel = new Border { Style = Ui.Res<Style>("Panel"), Padding = new Thickness(28), Child = facts };
            _columns.Children.Add(_factsPanel);

            _ratingPanel.Style = Ui.Res<Style>("Panel");
            _ratingPanel.Padding = new Thickness(28);
            _ratingPanel.Child = _ratingBody;
            Grid.SetColumn(_ratingPanel, 1);
            _columns.Children.Add(_ratingPanel);
            Root.Children.Add(_columns);

            // More like this.
            var similar = MovieStore.Instance.Catalog
                .Where(x => x.Id != movie.Id && x.Genres.Contains(movie.PrimaryGenre))
                .OrderByDescending(x => x.CommunityRating)
                .Take(8)
                .ToList();
            if (similar.Count > 0)
            {
                var row = new PosterRow();
                row.SetMovies(similar, 180);
                var section = new StackPanel { Margin = new Thickness(0, 64, 0, 0) };
                section.Children.Add(Components.SectionHeader($"More {movie.PrimaryGenre}", "More Like This", row.CreateArrows("similar movies")));
                section.Children.Add(row);
                Root.Children.Add(section);
            }
        }

        static void AddFact(StackPanel panel, string label, string value)
        {
            panel.Children.Add(Components.Text(label.ToUpperInvariant(), "Text.Eyebrow", new Thickness(0, 0, 0, 4)));
            var v = Components.Text(value, "Text.Body", new Thickness(0, 0, 0, 16));
            v.FontSize = 16;
            v.LineHeight = 25;
            panel.Children.Add(v);
        }

        void GoBack()
        {
            if (NavigationService?.CanGoBack == true) NavigationService.GoBack();
            else Shell.Home();
        }

        protected override void Render()
        {
            var entry = Store.Entry(_movie.Id);
            var m = _movie;

            _tags.Children.Clear();
            if (entry?.HighPriority == true) _tags.Children.Add(Components.Tag("High priority", Components.TagKind.HighPriority));
            if (entry?.IsWatched == true) _tags.Children.Add(Components.Tag("Watched", Components.TagKind.Watched));
            else if (entry != null) _tags.Children.Add(Components.Tag("Want to watch", Components.TagKind.Unwatched));
            if (entry?.OnWatchlist == true) _tags.Children.Add(Components.Tag("On watchlist", Components.TagKind.Unwatched));
            _tags.Visibility = _tags.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            _buttons.Children.Clear();
            void Add(Button b, string tag)
            {
                b.Margin = new Thickness(0, 0, 12, 12);
                b.Tag = tag;
                _buttons.Children.Add(b);
            }
            Add(entry?.OnWatchlist == true
                ? Components.ActionButton("On watchlist", "Btn.Done", Glyphs.Check, () => MovieActions.RemoveFromWatchlist(m), $"{m.Title} is on your watchlist. Remove it")
                : Components.ActionButton("Add to Watchlist", "Btn.Primary", Glyphs.Add, () => MovieActions.AddToWatchlist(m), $"Add {m.Title} to watchlist"), "d:list");
            Add(Components.ActionButton("Watch Trailer", "Btn.Secondary", Glyphs.Play, () => MovieActions.Trailer(m), $"Watch the {m.Title} trailer"), "d:trailer");
            Add(entry?.IsWatched == true
                ? Components.ActionButton("Watched", "Btn.Done", Glyphs.Check, () => MovieActions.ToggleWatched(m), $"{m.Title} is watched. Mark as unwatched")
                : Components.ActionButton("Already watched", "Btn.Tertiary", Glyphs.Check, () => MovieActions.ToggleWatched(m), $"I've already watched {m.Title}"), "d:watched");
            if (entry?.OnWatchlist == true)
            {
                var star = Components.IconButton(entry.HighPriority ? Glyphs.StarFill : Glyphs.Star,
                    entry.HighPriority ? "Remove high priority" : "Mark as high priority",
                    () => MovieActions.TogglePriority(m), active: entry.HighPriority);
                star.Height = star.Width = 46;
                Add(star, "d:priority");
            }

            _ratingBody.Children.Clear();
            _ratingBody.Children.Add(Components.Text("Your rating", "Text.Panel", new Thickness(0, 0, 0, 16)));
            if (entry?.IsWatched == true)
            {
                var stars = new StarRating(30) { Value = entry.UserRating ?? 0, Tag = "d:stars" };
                stars.ValueChanged += (_, v) => Store.SetRating(m.Id, v);
                _ratingBody.Children.Add(stars);
                _ratingBody.Children.Add(Components.Text(
                    entry.UserRating is double r ? $"{Format.Rating(r)} your rating" : "Tap a star to rate it.",
                    "Text.Meta", new Thickness(0, 10, 0, 4)));
                _ratingBody.Children.Add(Components.Text($"Watched {Format.RelativeTime(entry.WatchedAt ?? entry.AddedAt)}", "Text.Meta"));
            }
            else
            {
                var msg = Components.Text("Mark it as watched to rate it. Your ratings stay on this device.", "Text.Body");
                msg.FontSize = 15;
                msg.LineHeight = 23;
                msg.Foreground = Ui.Brush("TextMuted");
                _ratingBody.Children.Add(msg);
                var b = Components.ActionButton("Mark watched", "Btn.Light", Glyphs.Check, () => MovieActions.ToggleWatched(m));
                b.HorizontalAlignment = HorizontalAlignment.Left;
                b.Margin = new Thickness(0, 18, 0, 0);
                b.Tag = "d:mark";
                _ratingBody.Children.Add(b);
            }
        }

        protected override void ApplyLayout(Breakpoint bp)
        {
            var desktop = bp == Breakpoint.Desktop;
            var mobile = bp == Breakpoint.Mobile;
            _title.FontSize = desktop ? 76 : mobile ? 44 : 60;
            _title.LineHeight = _title.FontSize * 1.08;
            _heroInner.Margin = mobile ? new Thickness(24, 32, 24, 20) : new Thickness(56, 52, 56, 40);

            _columns.ColumnDefinitions[1].Width = desktop ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            Grid.SetColumn(_ratingPanel, desktop ? 1 : 0);
            Grid.SetRow(_ratingPanel, desktop ? 0 : 1);
            _ratingPanel.Margin = desktop ? new Thickness(24, 0, 0, 0) : new Thickness(0, 24, 0, 0);
            _ratingPanel.VerticalAlignment = VerticalAlignment.Top;
        }
    }
}
