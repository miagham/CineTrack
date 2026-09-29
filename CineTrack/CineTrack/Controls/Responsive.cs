namespace CineTrack.Controls
{
    public enum Breakpoint
    {
        Mobile,   // < 700px
        Tablet,   // 700–1099px
        Desktop   // ≥ 1100px
    }

    /// <summary>Window-width breakpoints from spec section 9. Updated by MainWindow on resize.</summary>
    public static class Responsive
    {
        public static Breakpoint Current { get; private set; } = Breakpoint.Desktop;
        public static double Width { get; private set; } = 1400;

        public static event EventHandler? Changed;

        public static bool IsDesktop => Current == Breakpoint.Desktop;
        public static bool IsMobile => Current == Breakpoint.Mobile;

        public static void Update(double width)
        {
            Width = width;
            var bp = width >= 1100 ? Breakpoint.Desktop : width >= 700 ? Breakpoint.Tablet : Breakpoint.Mobile;
            if (bp == Current) return;
            Current = bp;
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }
}
