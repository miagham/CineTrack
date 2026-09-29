using System.Windows;
using System.Windows.Media;

namespace CineTrack.Controls
{
    /// <summary>Attached properties used by the button and chip styles in Themes/Theme.xaml.</summary>
    public static class Ui
    {
        /// <summary>Leading icon glyph (Segoe Fluent Icons code point) for a styled button.</summary>
        public static readonly DependencyProperty GlyphProperty = DependencyProperty.RegisterAttached(
            "Glyph", typeof(string), typeof(Ui), new FrameworkPropertyMetadata(null));

        public static string? GetGlyph(DependencyObject d) => (string?)d.GetValue(GlyphProperty);
        public static void SetGlyph(DependencyObject d, string? value) => d.SetValue(GlyphProperty, value);

        /// <summary>Selected/active state: gold nav link, selected chip, high-priority star.</summary>
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.RegisterAttached(
            "IsActive", typeof(bool), typeof(Ui), new FrameworkPropertyMetadata(false));

        public static bool GetIsActive(DependencyObject d) => (bool)d.GetValue(IsActiveProperty);
        public static void SetIsActive(DependencyObject d, bool value) => d.SetValue(IsActiveProperty, value);

        /// <summary>Clips an element to a rounded rectangle that follows its size.</summary>
        public static readonly DependencyProperty ClipRadiusProperty = DependencyProperty.RegisterAttached(
            "ClipRadius", typeof(double), typeof(Ui), new FrameworkPropertyMetadata(0.0, OnClipRadiusChanged));

        public static double GetClipRadius(DependencyObject d) => (double)d.GetValue(ClipRadiusProperty);
        public static void SetClipRadius(DependencyObject d, double value) => d.SetValue(ClipRadiusProperty, value);

        static void OnClipRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement fe) return;
            fe.SizeChanged -= UpdateClip;
            fe.SizeChanged += UpdateClip;
            ApplyClip(fe);
        }

        static void UpdateClip(object sender, SizeChangedEventArgs e) => ApplyClip((FrameworkElement)sender);

        static void ApplyClip(FrameworkElement fe)
        {
            var r = GetClipRadius(fe);
            fe.Clip = new RectangleGeometry(new Rect(0, 0, fe.ActualWidth, fe.ActualHeight), r, r);
        }

        public static Brush Brush(string key) => (Brush)Application.Current.FindResource(key);
        public static T Res<T>(string key) => (T)Application.Current.FindResource(key);

        /// <summary>True when Windows "Show animations" is off — honors reduced-motion preferences.</summary>
        public static bool ReducedMotion => !SystemParameters.ClientAreaAnimation;
    }

    /// <summary>Segoe Fluent Icons / MDL2 code points used across the app.</summary>
    public static class Glyphs
    {
        public const string Search = "\uE721";
        public const string Add = "\uE710";
        public const string Play = "\uE768";
        public const string Check = "\uE73E";
        public const string Star = "\uE734";
        public const string StarFill = "\uE735";
        public const string Delete = "\uE74D";
        public const string Person = "\uE77B";
        public const string ChevronLeft = "\uE76B";
        public const string ChevronRight = "\uE76C";
        public const string Filter = "\uE71C";
        public const string Clock = "\uE823";
        public const string Grid = "\uE80A";
        public const string List = "\uE8FD";
        public const string Menu = "\uE700";
        public const string Close = "\uE711";
        public const string Tag = "\uE8EC";
        public const string Movies = "\uE8B2";
        public const string Bookmark = "\uE8A4";
        public const string Library = "\uE8F1";
        public const string Eye = "\uE890";
        public const string Back = "\uE72B";
        public const string Info = "\uE946";
    }
}
