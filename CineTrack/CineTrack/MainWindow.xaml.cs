using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using CineTrack.Controls;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>
    /// App shell: header, page frame, footer, plus the toast, rating prompt and drawer overlays.
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer _toastTimer = new();
        Action? _toastUndo;
        Action? _drawerClosed;
        IInputElement? _focusBeforeOverlay;

        public MainWindow()
        {
            InitializeComponent();
            HeaderSearch.SetLarge(false, false);
            _toastTimer.Tick += (_, _) => HideToast();

            SizeChanged += (_, _) => Responsive.Update(ActualWidth);
            Responsive.Changed += (_, _) => ApplyLayout();
            Responsive.Update(Width);
            ApplyLayout();

            PreviewKeyDown += OnPreviewKeyDown;
            Navigate(new HomePage());
        }

        // ---------- Navigation ----------

        public void Navigate(CinePage page)
        {
            MenuPopup.IsOpen = false;
            if (DrawerOverlay.Visibility == Visibility.Visible) CloseDrawer();
            PageFrame.Navigate(page);
        }

        void PageFrame_Navigated(object sender, NavigationEventArgs e)
        {
            MainScroll.ScrollToTop();
            var page = e.Content as CinePage;
            var key = page?.NavKey;
            Ui.SetIsActive(NavHome, key == "home");
            Ui.SetIsActive(NavDiscover, key == "discover");
            Ui.SetIsActive(NavMyMovies, key == "mymovies");
            Ui.SetIsActive(NavWatchlist, key == "watchlist");
            Ui.SetIsActive(ProfileButton, key == "profile");
            ProfileButton.BorderBrush = Ui.Brush(key == "profile" ? "Gold" : "TextMuted");
            ProfileButton.Foreground = Ui.Brush(key == "profile" ? "Gold" : "Cream");
            Title = page?.Title is { Length: > 0 } t ? $"{t} · CineTrack" : "CineTrack: Personal Movie Companion";
            ApplyLayout();
        }

        void Home_Click(object sender, RoutedEventArgs e) => Shell.Home();
        void Discover_Click(object sender, RoutedEventArgs e) => Shell.Discover();
        void MyMovies_Click(object sender, RoutedEventArgs e) => Shell.MyMovies();
        void Watchlist_Click(object sender, RoutedEventArgs e) => Shell.Watchlist();
        void Profile_Click(object sender, RoutedEventArgs e) => Shell.Profile();
        void About_Click(object sender, RoutedEventArgs e) => Shell.About();
        void Menu_Click(object sender, RoutedEventArgs e) => MenuPopup.IsOpen = !MenuPopup.IsOpen;

        void SearchIcon_Click(object sender, RoutedEventArgs e)
        {
            if (PageFrame.Content is HomePage home) home.FocusSearch();
            else if (PageFrame.Content is SearchPage search) search.FocusSearch();
            else Shell.Search("");
        }

        // ---------- Responsive header/footer (spec 9) ----------

        void ApplyLayout()
        {
            var bp = Responsive.Current;
            var mobile = bp == Breakpoint.Mobile;
            var onHome = PageFrame.Content is HomePage;
            var onSearch = PageFrame.Content is SearchPage;

            // Full header search on desktop (except Home, which has its own); an icon otherwise.
            var showBar = !onHome && !onSearch && Responsive.Width >= 900;
            HeaderSearch.Visibility = showBar ? Visibility.Visible : Visibility.Collapsed;
            SearchIconButton.Visibility = showBar ? Visibility.Collapsed : Visibility.Visible;

            NavLinks.Visibility = mobile ? Visibility.Collapsed : Visibility.Visible;
            ProfileButton.Visibility = mobile ? Visibility.Collapsed : Visibility.Visible;
            MenuButton.Visibility = mobile ? Visibility.Visible : Visibility.Collapsed;
            ProfileButton.Content = bp == Breakpoint.Desktop ? "Profile" : null;
            ProfileButton.Padding = bp == Breakpoint.Desktop ? new Thickness(18, 10, 18, 10) : new Thickness(12);
            AutomationProperties.SetName(ProfileButton, "Profile");

            var gutter = mobile ? 16 : 32;
            HeaderGrid.Margin = new Thickness(gutter, 0, gutter, 0);
            HeaderGrid.Height = mobile ? 64 : 76;
            ContentColumn.Padding = new Thickness(gutter, mobile ? 28 : 48, gutter, 0);
            LeftStripColumn.Width = RightStripColumn.Width = new GridLength(mobile ? 0 : 16);
            FooterGrid.Margin = new Thickness(gutter, 36, gutter, 40);

            // Footer stacks on narrow windows.
            Grid.SetColumn(FooterRight, mobile ? 0 : 1);
            Grid.SetRow(FooterRight, mobile ? 1 : 0);
            FooterRight.HorizontalAlignment = mobile ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            FooterRight.Margin = new Thickness(mobile ? -4 : 0, mobile ? 20 : 0, 0, 0);
            FooterLinks.HorizontalAlignment = FooterRight.HorizontalAlignment;
            Copyright.HorizontalAlignment = FooterRight.HorizontalAlignment;

            DrawerPanel.Width = Math.Min(340, Math.Max(280, ActualWidth - 40));
            if (bp == Breakpoint.Desktop && DrawerOverlay.Visibility == Visibility.Visible) CloseDrawer();
        }

        // ---------- Toast ----------

        public void ShowToast(string message, Action? undo)
        {
            ToastText.Text = message;
            _toastUndo = undo;
            ToastUndo.Visibility = undo == null ? Visibility.Collapsed : Visibility.Visible;
            AutomationProperties.SetName(ToastUndo, $"Undo: {message}");
            Toast.Visibility = Visibility.Visible;
            Toast.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(Ui.ReducedMotion ? 0 : 160)));

            var peer = UIElementAutomationPeer.FromElement(ToastText) ?? UIElementAutomationPeer.CreatePeerForElement(ToastText);
            peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);

            _toastTimer.Stop();
            _toastTimer.Interval = TimeSpan.FromSeconds(undo == null ? 3.5 : 6);
            _toastTimer.Start();
        }

        void HideToast()
        {
            _toastTimer.Stop();
            _toastUndo = null;
            var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(Ui.ReducedMotion ? 0 : 200));
            fade.Completed += (_, _) => { if (Toast.Opacity == 0) Toast.Visibility = Visibility.Collapsed; };
            Toast.BeginAnimation(OpacityProperty, fade);
        }

        void ToastUndo_Click(object sender, RoutedEventArgs e)
        {
            var undo = _toastUndo;
            HideToast();
            undo?.Invoke();
        }

        // ---------- Rating prompt (spec 8: after marking watched, skippable) ----------

        public void ShowRatingPrompt(Movie movie)
        {
            _focusBeforeOverlay = Keyboard.FocusedElement;
            var store = MovieStore.Instance;
            RatingContent.Children.Clear();

            RatingContent.Children.Add(Components.Text("MARKED AS WATCHED", "Text.Eyebrow", new Thickness(0, 0, 0, 8)));
            var title = Components.Text($"How was {movie.Title}?", "Text.Panel");
            title.FontSize = 28;
            RatingContent.Children.Add(title);
            RatingContent.Children.Add(Components.Text(Format.Meta(movie, withRuntime: true), "Text.Meta", new Thickness(0, 6, 0, 22)));

            var stars = new StarRating(36) { Value = store.Entry(movie.Id)?.UserRating ?? 0 };
            RatingContent.Children.Add(stars);
            var label = Components.Text("Tap a star — half stars count too.", "Text.Meta", new Thickness(0, 10, 0, 26));
            RatingContent.Children.Add(label);

            var buttons = new WrapPanel();
            var save = Components.ActionButton("Save rating", "Btn.Primary", Glyphs.StarFill, () =>
            {
                store.SetRating(movie.Id, stars.Value);
                CloseRatingPrompt();
                ShowToast($"Rated {movie.Title} ★ {Format.Rating(stars.Value)}", null);
            });
            save.IsEnabled = stars.Value > 0;
            save.Margin = new Thickness(0, 0, 10, 8);
            var skip = Components.ActionButton("Skip", "Btn.Secondary", null, () =>
            {
                CloseRatingPrompt();
                ShowToast($"Marked {movie.Title} as watched", null);
            });
            skip.Margin = new Thickness(0, 0, 0, 8);
            buttons.Children.Add(save);
            buttons.Children.Add(skip);
            RatingContent.Children.Add(buttons);

            stars.ValueChanged += (_, v) =>
            {
                save.IsEnabled = v > 0;
                label.Text = $"{Format.Rating(v)} out of 5";
            };

            RatingOverlay.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(() => stars.Focus(), DispatcherPriority.Input);
        }

        void CloseRatingPrompt()
        {
            RatingOverlay.Visibility = Visibility.Collapsed;
            RatingContent.Children.Clear();
            RestoreFocus();
        }

        // ---------- Drawer ----------

        public void OpenDrawer(FrameworkElement content, string title, Action? onClosed)
        {
            _focusBeforeOverlay = Keyboard.FocusedElement;
            _drawerClosed = onClosed;
            DrawerTitle.Text = title;
            DrawerHost.Content = content;
            DrawerOverlay.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(() => DrawerPanel.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)),
                DispatcherPriority.Input);
        }

        public void CloseDrawer()
        {
            if (DrawerOverlay.Visibility != Visibility.Visible) return;
            DrawerHost.Content = null;
            DrawerOverlay.Visibility = Visibility.Collapsed;
            var closed = _drawerClosed;
            _drawerClosed = null;
            closed?.Invoke();
            RestoreFocus();
        }

        void DrawerScrim_Click(object sender, MouseButtonEventArgs e) => CloseDrawer();
        void DrawerClose_Click(object sender, RoutedEventArgs e) => CloseDrawer();

        void RestoreFocus()
        {
            if (_focusBeforeOverlay is UIElement { IsVisible: true } el) el.Focus();
            _focusBeforeOverlay = null;
        }

        void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            if (RatingOverlay.Visibility == Visibility.Visible)
            {
                CloseRatingPrompt();
                e.Handled = true;
            }
            else if (DrawerOverlay.Visibility == Visibility.Visible)
            {
                CloseDrawer();
                e.Handled = true;
            }
        }
    }
}
