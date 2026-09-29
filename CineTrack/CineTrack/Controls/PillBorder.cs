using System.Windows;
using System.Windows.Controls;

namespace CineTrack.Controls
{
    /// <summary>
    /// A Border with fully rounded ends. WPF doesn't clamp oversized corner radii the way CSS
    /// does (a radius of 999 draws an ellipse), so the radius follows the element's height.
    /// </summary>
    public class PillBorder : Border
    {
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var r = Math.Min(ActualWidth, ActualHeight) / 2;
            if (CornerRadius.TopLeft != r) CornerRadius = new CornerRadius(r);
        }
    }
}
