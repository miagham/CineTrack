using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CineTrack.Controls;
using CineTrack.Services;

namespace CineTrack
{
    /// <summary>
    /// Base class for every screen. Re-renders when the user's data changes and
    /// re-lays out when the window crosses a responsive breakpoint.
    /// </summary>
    public class CinePage : Page
    {
        public CinePage()
        {
            FontFamily = Ui.Res<System.Windows.Media.FontFamily>("Manrope");
            Foreground = Ui.Brush("Cream");
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>Which header nav link is highlighted while this page is shown.</summary>
        public virtual string? NavKey => null;

        protected MovieStore Store => MovieStore.Instance;

        /// <summary>Rebuild anything that depends on the user's data.</summary>
        protected virtual void Render() { }

        /// <summary>Adjust layout for the current breakpoint.</summary>
        protected virtual void ApplyLayout(Breakpoint bp) { }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Store.Changed += OnStoreChanged;
            Responsive.Changed += OnResponsiveChanged;
            ApplyLayout(Responsive.Current);
            Render();
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Store.Changed -= OnStoreChanged;
            Responsive.Changed -= OnResponsiveChanged;
        }

        /// <summary>
        /// Re-renders, then puts keyboard focus back on the rebuilt control with the same Tag
        /// (action buttons are tagged "action:movieId") so keyboard users don't lose their place.
        /// </summary>
        void OnStoreChanged(object? sender, EventArgs e)
        {
            var key = (Keyboard.FocusedElement as FrameworkElement)?.Tag as string;
            Render();
            if (key == null) return;
            Dispatcher.BeginInvoke(() => FindByTag(this, key)?.Focus(), DispatcherPriority.Loaded);
        }

        static FrameworkElement? FindByTag(DependencyObject root, string key)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is FrameworkElement { Focusable: true, IsVisible: true } fe && Equals(fe.Tag, key)) return fe;
                var found = FindByTag(child, key);
                if (found != null) return found;
            }
            return null;
        }

        void OnResponsiveChanged(object? sender, EventArgs e) => ApplyLayout(Responsive.Current);
    }
}
