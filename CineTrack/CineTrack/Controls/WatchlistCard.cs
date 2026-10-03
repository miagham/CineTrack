using System.Windows;
using System.Windows.Controls;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack.Controls
{
    /// <summary>Horizontal list card used on the Watchlist.</summary>
    public static class WatchlistCard
    {
        public static Border Create(Movie m, UserMovieEntry entry, bool compact)
        {
            var posterW = compact ? 76 : 96;
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var poster = Components.Poster(m, posterW, posterW * 1.5, 10);
            poster.VerticalAlignment = VerticalAlignment.Top;
            Components.MakeClickable(poster, $"Open details for {m.Title}", () => Shell.Details(m));
            grid.Children.Add(poster);

            var body = new StackPanel { Margin = new Thickness(16, 0, 0, 0) };
            Grid.SetColumn(body, 1);
            grid.Children.Add(body);

            var tags = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
            if (entry.HighPriority) tags.Children.Add(Components.Tag("High priority", Components.TagKind.HighPriority));
            tags.Children.Add(entry.IsWatched
                ? Components.Tag("Watched", Components.TagKind.Watched)
                : Components.Tag("Unwatched", Components.TagKind.Unwatched));
            body.Children.Add(tags);

            var title = Components.Text(m.Title, "Text.CardTitle");
            title.FontSize = 20;
            title.TextTrimming = TextTrimming.None;
            title.TextWrapping = TextWrapping.Wrap;
            body.Children.Add(title);
            body.Children.Add(Components.Text(Format.Meta(m, withRuntime: true), "Text.Meta", new Thickness(0, 4, 0, 0)));
            body.Children.Add(Components.Text($"Added {Format.RelativeTime(entry.AddedAt)}", "Text.Meta", new Thickness(0, 4, 0, 14)));

            var actions = new WrapPanel();
            var mark = entry.IsWatched
                ? Components.ActionButton("Watched", "Btn.Done", Glyphs.Check, () => MovieActions.ToggleWatched(m), $"{m.Title} is watched. Mark as unwatched")
                : Components.ActionButton("Mark watched", "Btn.Light", Glyphs.Check, () => MovieActions.ToggleWatched(m), $"Mark {m.Title} as watched");
            mark.Tag = $"watched:{m.Id}";
            mark.Padding = new Thickness(14, 9, 14, 9);
            mark.FontSize = 13;
            mark.Margin = new Thickness(0, 0, 8, 8);
            actions.Children.Add(mark);

            var star = Components.IconButton(entry.HighPriority ? Glyphs.StarFill : Glyphs.Star,
                entry.HighPriority ? $"Remove high priority from {m.Title}" : $"Mark {m.Title} as high priority",
                () => MovieActions.TogglePriority(m), small: false, active: entry.HighPriority);
            star.Tag = $"priority:{m.Id}";
            star.Margin = new Thickness(0, 0, 8, 8);
            actions.Children.Add(star);

            var trash = Components.IconButton(Glyphs.Delete, $"Remove {m.Title} from watchlist", () => MovieActions.RemoveFromWatchlist(m));
            trash.Margin = new Thickness(0, 0, 0, 8);
            actions.Children.Add(trash);
            body.Children.Add(actions);

            return new Border
            {
                Style = Ui.Res<Style>("Panel"),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 16, 16),
                Child = grid,
            };
        }
    }
}
