using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CineTrack.Controls
{
    /// <summary>
    /// Five gold stars with half-star precision (0.5–5). Mouse: hover to preview, click to set.
    /// Keyboard: ←/→ by half a star, Home/End, number keys 1–5.
    /// </summary>
    public class StarRating : Grid
    {
        readonly StackPanel _stars = new() { Orientation = Orientation.Horizontal };
        readonly List<RectangleGeometry> _fills = new();
        double _value;
        double? _preview;

        public StarRating(double starSize = 32, bool readOnly = false)
        {
            StarSize = starSize;
            IsReadOnly = readOnly;
            Background = Brushes.Transparent;
            HorizontalAlignment = HorizontalAlignment.Left;
            Children.Add(_stars);

            for (var i = 0; i < 5; i++)
            {
                var cell = new Grid { Width = starSize, Height = starSize, Margin = new Thickness(0, 0, starSize * 0.15, 0) };
                cell.Children.Add(StarGlyph("\uE734", "MutedGold"));
                var fill = StarGlyph("\uE735", "Gold");
                var clip = new RectangleGeometry(new Rect(0, 0, 0, starSize));
                fill.Clip = clip;
                _fills.Add(clip);
                cell.Children.Add(fill);
                _stars.Children.Add(cell);
            }

            if (!readOnly)
            {
                Cursor = Cursors.Hand;
                Focusable = true;
                FocusVisualStyle = Ui.Res<Style>("GoldFocusCard");
                MouseMove += (_, e) => { _preview = ValueAt(e.GetPosition(_stars).X); Draw(); };
                MouseLeave += (_, _) => { _preview = null; Draw(); };
                MouseLeftButtonUp += (_, e) => { Value = ValueAt(e.GetPosition(_stars).X); e.Handled = true; };
            }
            AutomationProperties.SetName(this, "Your rating");
            Draw();
        }

        public double StarSize { get; }
        public bool IsReadOnly { get; }

        public event EventHandler<double>? ValueChanged;

        public double Value
        {
            get => _value;
            set
            {
                var v = Math.Clamp(Math.Round(value * 2) / 2, 0, 5);
                if (v == _value) return;
                _value = v;
                Draw();
                AutomationProperties.SetHelpText(this, v == 0 ? "Not rated" : $"{v:0.0} of 5 stars");
                ValueChanged?.Invoke(this, v);
            }
        }

        TextBlock StarGlyph(string glyph, string brush) => new()
        {
            Text = glyph,
            FontFamily = Ui.Res<FontFamily>("Icons"),
            FontSize = StarSize * 0.9,
            Foreground = Ui.Brush(brush),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = StarSize,
            TextAlignment = TextAlignment.Center,
        };

        double ValueAt(double x)
        {
            var step = StarSize * 1.15;
            var raw = x / step;
            var star = Math.Floor(raw);
            var within = (raw - star) * step / StarSize;   // 0–1 inside the star, >1 in the gap
            var v = star + (within <= 0.5 ? 0.5 : 1);
            return Math.Clamp(v, 0.5, 5);
        }

        void Draw()
        {
            var shown = _preview ?? _value;
            for (var i = 0; i < 5; i++)
            {
                var fraction = Math.Clamp(shown - i, 0, 1);
                _fills[i].Rect = new Rect(0, 0, StarSize * fraction, StarSize);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (IsReadOnly) return;
            switch (e.Key)
            {
                case Key.Right or Key.Up: Value = Math.Max(0.5, Value + 0.5); break;
                case Key.Left or Key.Down: Value = Math.Max(0.5, Value - 0.5); break;
                case Key.Home: Value = 0.5; break;
                case Key.End: Value = 5; break;
                case >= Key.D1 and <= Key.D5: Value = e.Key - Key.D0; break;
                case >= Key.NumPad1 and <= Key.NumPad5: Value = e.Key - Key.NumPad0; break;
                default: return;
            }
            e.Handled = true;
        }
    }
}
