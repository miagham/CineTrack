using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack.Controls
{
    /// <summary>
    /// Vertical movie poster card. Hover, keyboard focus, or a first tap reveals
    /// the overlay with "+ Watchlist" and "View details". Status tag always reflects the user's data.
    /// </summary>
    public partial class PosterCard : UserControl
    {
        readonly Movie _movie;
        bool _overlayShown;
        bool _overlayWasShownOnPress;

        public PosterCard(Movie movie, double width = 180)
        {
            InitializeComponent();
            _movie = movie;
            Width = width;
            PosterFrame.Height = (width - 18) * 1.5;
            PosterHost.Content = Components.PosterArt(movie, titleSize: Math.Round(width / 10.5));
            TitleText.Text = movie.Title;
            MetaText.Text = Format.Meta(movie);
            RatingHost.Content = Components.RatingLabel(movie.CommunityRating);
            OverlayMeta.Text = $"{Format.Runtime(movie.RuntimeMin)}{Format.Dot}{movie.PrimaryGenre}";
            Margin = new Thickness(0, 0, 16, 0);

            Loaded += (_, _) => { MovieStore.Instance.Changed += OnStoreChanged; Refresh(); };
            Unloaded += (_, _) => MovieStore.Instance.Changed -= OnStoreChanged;
            MouseEnter += (_, _) => UpdateOverlay();
            MouseLeave += (_, _) => UpdateOverlay();
            IsKeyboardFocusWithinChanged += (_, _) => UpdateOverlay();
        }

        public Movie Movie => _movie;

        void OnStoreChanged(object? sender, EventArgs e) => Refresh();

        void Refresh()
        {
            var entry = MovieStore.Instance.Entry(_movie.Id);

            TagHost.Content = entry == null ? null
                : entry.IsWatched ? Components.Tag("Watched", Components.TagKind.Watched)
                : Components.Tag("Want to watch", Components.TagKind.Unwatched);

            var onList = entry?.OnWatchlist == true;
            WatchlistButton.Content = onList ? "On Watchlist" : "Watchlist";
            Ui.SetGlyph(WatchlistButton, onList ? Glyphs.Check : Glyphs.Add);
            WatchlistButton.Style = Ui.Res<Style>(onList ? "Btn.Done" : "Btn.Primary");
            AutomationProperties.SetName(WatchlistButton, onList ? $"{_movie.Title} is on your watchlist. Open watchlist" : $"Add {_movie.Title} to watchlist");
            AutomationProperties.SetName(DetailsButton, $"View details for {_movie.Title}");

            var status = entry == null ? "" : entry.IsWatched ? ", watched" : ", want to watch";
            AutomationProperties.SetName(this, $"{_movie.Title}, {Format.Meta(_movie)}, rated {Format.Rating(_movie.CommunityRating)}{status}");
        }

        void UpdateOverlay()
        {
            var show = IsMouseOver || IsKeyboardFocusWithin;
            if (show == _overlayShown) return;
            _overlayShown = show;

            var reduced = Ui.ReducedMotion;
            var duration = TimeSpan.FromMilliseconds(reduced ? 0 : 180);

            Card.BorderBrush = show ? Ui.Brush("Gold") : Ui.Brush("CreamFaint");
            Card.Effect = show
                ? new DropShadowEffect { Color = Ui.Res<Color>("GoldColor"), BlurRadius = 22, ShadowDepth = 0, Opacity = 0.35 }
                : null;

            if (show) Overlay.Visibility = Visibility.Visible;
            var fade = new DoubleAnimation(show ? 1 : 0, duration);
            if (!show) fade.Completed += (_, _) => { if (!_overlayShown) Overlay.Visibility = Visibility.Collapsed; };
            Overlay.BeginAnimation(OpacityProperty, fade);

            if (reduced) return;
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            Lift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(show ? -4 : 0, duration) { EasingFunction = ease });
            Zoom.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(show ? 1.05 : 1, TimeSpan.FromMilliseconds(320)) { EasingFunction = ease });
            Zoom.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(show ? 1.05 : 1, TimeSpan.FromMilliseconds(320)) { EasingFunction = ease });
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _overlayWasShownOnPress = _overlayShown && Overlay.Opacity > 0.5;
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (e.Handled) return;
            e.Handled = true;

            // Touch: the first tap reveals the actions; a tap on the revealed card opens details.
            if (e.StylusDevice != null && !_overlayWasShownOnPress)
            {
                Focus();
                return;
            }
            Shell.Details(_movie);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.OriginalSource == this && e.Key is Key.Enter or Key.Space)
            {
                e.Handled = true;
                Shell.Details(_movie);
            }
        }

        void WatchlistButton_Click(object sender, RoutedEventArgs e)
        {
            if (MovieStore.Instance.Entry(_movie.Id)?.OnWatchlist == true) Shell.Watchlist();
            else MovieActions.AddToWatchlist(_movie);
        }

        void DetailsButton_Click(object sender, RoutedEventArgs e) => Shell.Details(_movie);
    }
}
