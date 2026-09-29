using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CineTrack.Models;

namespace CineTrack.Controls
{
    /// <summary>
    /// Horizontal ScrollViewer that lets vertical mouse-wheel scrolling pass through to the page
    /// (Shift+wheel scrolls sideways) and supports touch swiping.
    /// </summary>
    public class HorizontalScroller : ScrollViewer
    {
        DispatcherTimer? _timer;

        public HorizontalScroller()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            PanningMode = PanningMode.HorizontalOnly;
            Focusable = false;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                ScrollToHorizontalOffset(HorizontalOffset - e.Delta);
                e.Handled = true;
            }
            // Otherwise leave the event unhandled so the page scrolls.
        }

        /// <summary>Scrolls by most of a viewport in the given direction (-1 or 1), smoothly unless motion is reduced.</summary>
        public void ScrollPage(int direction)
        {
            var target = Math.Clamp(HorizontalOffset + direction * ViewportWidth * 0.85, 0, ScrollableWidth);
            _timer?.Stop();
            if (Ui.ReducedMotion)
            {
                ScrollToHorizontalOffset(target);
                return;
            }
            var start = HorizontalOffset;
            var began = DateTime.Now;
            _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(15) };
            _timer.Tick += (_, _) =>
            {
                var t = Math.Min(1, (DateTime.Now - began).TotalMilliseconds / 320);
                var eased = 1 - Math.Pow(1 - t, 3);
                ScrollToHorizontalOffset(start + (target - start) * eased);
                if (t >= 1) _timer!.Stop();
            };
            _timer.Start();
        }
    }

    /// <summary>A carousel row of poster cards with ‹ › controls (spec 5.10).</summary>
    public class PosterRow : HorizontalScroller
    {
        readonly StackPanel _panel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 6) };
        Button? _prev, _next;

        public PosterRow()
        {
            Content = _panel;
            ScrollChanged += (_, _) => UpdateArrows();
        }

        public void SetMovies(IEnumerable<Movie> movies, double cardWidth)
        {
            _panel.Children.Clear();
            foreach (var m in movies) _panel.Children.Add(new PosterCard(m, cardWidth));
            if (_panel.Children.Count > 0) ((FrameworkElement)_panel.Children[^1]).Margin = new Thickness(0);
            ScrollToHorizontalOffset(0);
        }

        public int Count => _panel.Children.Count;

        /// <summary>Builds the ‹ › buttons that drive this row.</summary>
        public StackPanel CreateArrows(string rowName)
        {
            _prev = Components.IconButton(Glyphs.ChevronLeft, $"Scroll {rowName} left", () => ScrollPage(-1));
            _next = Components.IconButton(Glyphs.ChevronRight, $"Scroll {rowName} right", () => ScrollPage(1));
            _prev.Margin = new Thickness(12, 0, 8, 0);
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(_prev);
            panel.Children.Add(_next);
            UpdateArrows();
            return panel;
        }

        void UpdateArrows()
        {
            if (_prev == null || _next == null) return;
            _prev.IsEnabled = HorizontalOffset > 1;
            _next.IsEnabled = HorizontalOffset < ScrollableWidth - 1;
        }
    }
}
