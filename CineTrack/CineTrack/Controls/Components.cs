using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CineTrack.Models;
using CineTrack.Services;

namespace CineTrack.Controls
{
    /// <summary>
    /// Small building blocks, built in code so pages can compose them freely.
    /// </summary>
    public static class Components
    {
        // ---------- Text ----------

        public static TextBlock Text(string text, string style, Thickness? margin = null) => new()
        {
            Text = text,
            Style = Ui.Res<Style>(style),
            Margin = margin ?? new Thickness(0),
        };

        public static TextBlock Icon(string glyph, double size, Brush? brush = null) => new()
        {
            Text = glyph,
            Style = Ui.Res<Style>("Text.Icon"),
            FontSize = size,
            Foreground = brush ?? Ui.Brush("Gold"),
        };

        /// <summary>Page intro: gold eyebrow, Playfair page title, subtitle.</summary>
        public static StackPanel PageHeader(string eyebrow, string title, string? subtitle)
        {
            var panel = new StackPanel();
            panel.Children.Add(Text(eyebrow.ToUpperInvariant(), "Text.Eyebrow", new Thickness(0, 0, 0, 10)));
            panel.Children.Add(Text(title, "Text.PageTitle"));
            if (subtitle != null)
            {
                var sub = Text(subtitle, "Text.Body", new Thickness(0, 10, 0, 0));
                sub.MaxWidth = 620;
                sub.HorizontalAlignment = HorizontalAlignment.Left;
                panel.Children.Add(sub);
            }
            return panel;
        }

        public static readonly string[] Suggestions =
        {
            "Christopher Nolan", "Sci-Fi under 2 hours", "Best Picture winners", "A24", "Denis Villeneuve"
        };

        /// <summary>"Try:" followed by suggestion chips that run a search.</summary>
        public static WrapPanel SuggestionChips(Action<string> run)
        {
            var wrap = new WrapPanel { Margin = new Thickness(0, 18, 0, 0) };
            var label = Text("Try:", "Text.Meta", new Thickness(0, 0, 12, 10));
            label.VerticalAlignment = VerticalAlignment.Center;
            label.Foreground = Ui.Brush("Cream");
            label.FontWeight = FontWeights.Bold;
            wrap.Children.Add(label);
            foreach (var s in Suggestions)
            {
                var chip = new Button { Content = s, Style = Ui.Res<Style>("Chip.Suggestion") };
                AutomationProperties.SetName(chip, $"Search for {s}");
                chip.Click += (_, _) => run(s);
                wrap.Children.Add(chip);
            }
            return wrap;
        }

        // ---------- Poster art ----------

        /// <summary>
        /// A poster: the real image when <see cref="Movie.PosterUrl"/> is set, otherwise a tinted
        /// vertical gradient with the title overlaid in Playfair.
        /// </summary>
        public static FrameworkElement PosterArt(Movie m, double titleSize = 18, bool showTitle = true)
        {
            var grid = new Grid();
            var tint = (Color)ColorConverter.ConvertFromString(m.Tint);
            var nearBlack = Ui.Res<Color>("NearBlackColor");
            var cream = Ui.Res<Color>("CreamColor");

            grid.Background = new LinearGradientBrush(new GradientStopCollection
            {
                new(Mix(tint, cream, 0.12), 0),
                new(tint, 0.4),
                new(Mix(tint, nearBlack, 0.8), 1),
            }, 90);

            // Soft projector light in the top corner.
            grid.Children.Add(new Border
            {
                Background = new RadialGradientBrush(Color.FromArgb(0x38, cream.R, cream.G, cream.B), Color.FromArgb(0, cream.R, cream.G, cream.B))
                {
                    Center = new Point(0.85, 0.1),
                    GradientOrigin = new Point(0.85, 0.1),
                    RadiusX = 0.9,
                    RadiusY = 0.55,
                },
            });

            if (!string.IsNullOrEmpty(m.PosterUrl))
            {
                grid.Children.Add(new Image
                {
                    Source = new BitmapImage(new Uri(m.PosterUrl, UriKind.RelativeOrAbsolute)),
                    Stretch = Stretch.UniformToFill,
                });
            }

            if (showTitle)
            {
                grid.Children.Add(new Border
                {
                    Background = new LinearGradientBrush(new GradientStopCollection
                    {
                        new(Color.FromArgb(0, nearBlack.R, nearBlack.G, nearBlack.B), 0.35),
                        new(Color.FromArgb(0xE6, nearBlack.R, nearBlack.G, nearBlack.B), 1),
                    }, 90),
                });
                grid.Children.Add(new TextBlock
                {
                    Text = m.Title,
                    FontFamily = Ui.Res<FontFamily>("Playfair"),
                    FontWeight = FontWeights.ExtraBold,
                    FontSize = titleSize,
                    Foreground = Ui.Brush("Cream"),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(titleSize * 0.6, 0, titleSize * 0.6, titleSize * 0.6),
                    LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                    LineHeight = titleSize * 1.15,
                });
            }

            AutomationProperties.SetName(grid, $"{m.Title} poster");
            return grid;
        }

        /// <summary>Poster art sized and clipped to rounded corners.</summary>
        public static FrameworkElement Poster(Movie m, double width, double height, double radius = 10, double titleSize = 0)
        {
            var art = PosterArt(m, titleSize, titleSize > 0);
            var frame = new Border { Width = width, Height = height, Child = art };
            Ui.SetClipRadius(frame, radius);
            return frame;
        }

        static Color Mix(Color a, Color b, double t) => Color.FromRgb(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t));

        // ---------- Tags ----------

        public enum TagKind { Watched, Unwatched, HighPriority, Badge }

        public static PillBorder Tag(string text, TagKind kind)
        {
            var gold = kind is TagKind.Watched or TagKind.HighPriority;
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            if (kind == TagKind.HighPriority)
            {
                content.Children.Add(new TextBlock
                {
                    Text = Glyphs.StarFill,
                    FontFamily = Ui.Res<FontFamily>("Icons"),
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Ui.Brush("NearBlack"),
                });
            }
            content.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = kind == TagKind.Badge ? 10.5 : 11.5,
                FontWeight = FontWeights.ExtraBold,
                Foreground = gold ? Ui.Brush("NearBlack") : kind == TagKind.Badge ? Ui.Brush("Gold") : Ui.Brush("Cream"),
                VerticalAlignment = VerticalAlignment.Center,
            });
            return new PillBorder
            {
                Child = content,
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0, 0, 6, 0),
                Background = gold ? Ui.Brush("Gold") : Ui.Brush("NearBlack"),
                BorderBrush = gold ? Ui.Brush("Gold") : kind == TagKind.Badge ? Ui.Brush("MutedGold") : Ui.Brush("TextMuted"),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        /// <summary>"★ 4.7" in gold.</summary>
        public static StackPanel RatingLabel(double rating, string? suffix = null, double size = 13)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            panel.Children.Add(new TextBlock
            {
                Text = Glyphs.StarFill,
                FontFamily = Ui.Res<FontFamily>("Icons"),
                FontSize = size - 2,
                Foreground = Ui.Brush("Gold"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0),
            });
            panel.Children.Add(new TextBlock
            {
                Text = Format.Rating(rating),
                Style = Ui.Res<Style>("Text.Rating"),
                FontSize = size,
            });
            if (suffix != null)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = " " + suffix,
                    Style = Ui.Res<Style>("Text.Meta"),
                    FontSize = size,
                });
            }
            AutomationProperties.SetName(panel, suffix == null ? $"Rated {Format.Rating(rating)} of 5" : $"{Format.Rating(rating)} of 5, {suffix}");
            return panel;
        }

        // ---------- Section header: eyebrow → heading → right-aligned actions ----------

        public static Grid SectionHeader(string eyebrow, string title, UIElement? actions = null, string headingStyle = "Text.Section")
        {
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var left = new StackPanel();
            if (!string.IsNullOrEmpty(eyebrow)) left.Children.Add(Text(eyebrow.ToUpperInvariant(), "Text.Eyebrow", new Thickness(0, 0, 0, 6)));
            left.Children.Add(Text(title, headingStyle));
            grid.Children.Add(left);
            if (actions is FrameworkElement fe)
            {
                fe.VerticalAlignment = VerticalAlignment.Bottom;
                Grid.SetColumn(fe, 1);
                grid.Children.Add(fe);
            }
            return grid;
        }

        public static Button LinkButton(string text, Action onClick, string? automationName = null)
        {
            var b = new Button { Content = text + "  ›", Style = Ui.Res<Style>("Btn.Link") };
            if (automationName != null) AutomationProperties.SetName(b, automationName);
            b.Click += (_, _) => onClick();
            return b;
        }

        public static Button IconButton(string glyph, string label, Action onClick, bool small = false, bool active = false)
        {
            var b = new Button { Style = Ui.Res<Style>(small ? "Btn.IconSmall" : "Btn.Icon"), ToolTip = label };
            Ui.SetGlyph(b, glyph);
            Ui.SetIsActive(b, active);
            AutomationProperties.SetName(b, label);
            b.Click += (_, _) => onClick();
            return b;
        }

        public static Button ActionButton(string text, string style, string? glyph, Action onClick, string? automationName = null)
        {
            var b = new Button { Content = text, Style = Ui.Res<Style>(style) };
            Ui.SetGlyph(b, glyph);
            if (automationName != null) AutomationProperties.SetName(b, automationName);
            b.Click += (_, _) => onClick();
            return b;
        }

        // ---------- Stat card ----------

        public static Border StatCard(string glyph, string label, string value)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var tile = new Border
            {
                Width = 52,
                Height = 52,
                CornerRadius = new CornerRadius(12),
                BorderBrush = Ui.Brush("MutedGold"),
                BorderThickness = new Thickness(1),
                Background = Ui.Brush("DarkBurgundy"),
                Child = new TextBlock
                {
                    Text = glyph,
                    FontFamily = Ui.Res<FontFamily>("Icons"),
                    FontSize = 20,
                    Foreground = Ui.Brush("Gold"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                VerticalAlignment = VerticalAlignment.Center,
            };
            grid.Children.Add(tile);

            var text = new StackPanel { Margin = new Thickness(16, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            text.Children.Add(Text(label, "Text.Meta"));
            text.Children.Add(Text(value, "Text.Stat", new Thickness(0, 2, 0, 0)));
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            var card = new Border
            {
                Style = Ui.Res<Style>("Panel"),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 16, 16),
                Child = grid,
            };
            AutomationProperties.SetName(card, $"{label}: {value}");
            return card;
        }

        // ---------- Progress bar row ----------

        public static FrameworkElement ProgressRow(string label, int count, double fraction, string unitOne = "movie", string unitMany = "movies")
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            var top = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
            var countText = Text(Format.Plural(count, unitOne, unitMany), "Text.Meta");
            DockPanel.SetDock(countText, Dock.Right);
            top.Children.Add(countText);
            var labelText = Text(label, "Text.CardTitle");
            labelText.FontSize = 14;
            top.Children.Add(labelText);
            panel.Children.Add(top);

            var track = new Grid { Height = 8 };
            track.Children.Add(new Border { Background = Ui.Brush("Track"), CornerRadius = new CornerRadius(4) });
            var fill = new Border { Background = Ui.Brush("Gold"), CornerRadius = new CornerRadius(4), HorizontalAlignment = HorizontalAlignment.Left };
            track.SizeChanged += (_, e) => fill.Width = Math.Max(8, e.NewSize.Width * Math.Clamp(fraction, 0, 1));
            track.Children.Add(fill);
            panel.Children.Add(track);
            AutomationProperties.SetName(panel, $"{label}: {Format.Plural(count, unitOne, unitMany)}");
            return panel;
        }

        // ---------- Empty state ----------

        public static Border EmptyState(string glyph, string title, string message, string? actionText = null, Action? action = null)
        {
            var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, MaxWidth = 440 };
            var icon = Icon(glyph, 28);
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            icon.Margin = new Thickness(0, 0, 0, 14);
            panel.Children.Add(icon);
            var t = Text(title, "Text.Panel");
            t.TextAlignment = TextAlignment.Center;
            t.FontSize = 22;
            panel.Children.Add(t);
            var body = Text(message, "Text.Body", new Thickness(0, 8, 0, 0));
            body.FontSize = 15;
            body.LineHeight = 23;
            body.Foreground = Ui.Brush("TextMuted");
            body.TextAlignment = TextAlignment.Center;
            panel.Children.Add(body);
            if (actionText != null && action != null)
            {
                var b = ActionButton(actionText, "Btn.Primary", Glyphs.Add, action);
                b.HorizontalAlignment = HorizontalAlignment.Center;
                b.Margin = new Thickness(0, 20, 0, 0);
                panel.Children.Add(b);
            }
            return new Border
            {
                Style = Ui.Res<Style>("Panel"),
                Padding = new Thickness(28, 36, 28, 36),
                Child = panel,
            };
        }

        // ---------- Clickable surfaces ----------

        /// <summary>Makes a non-button surface act like a link: mouse, Enter/Space, gold focus ring.</summary>
        public static void MakeClickable(FrameworkElement element, string automationName, Action onClick)
        {
            element.Cursor = Cursors.Hand;
            element.Focusable = true;
            KeyboardNavigation.SetIsTabStop(element, true);
            element.FocusVisualStyle = Ui.Res<Style>("GoldFocusCard");
            AutomationProperties.SetName(element, automationName);
            element.MouseLeftButtonUp += (_, e) =>
            {
                if (e.Handled) return;
                e.Handled = true;
                onClick();
            };
            element.KeyDown += (_, e) =>
            {
                if (e.OriginalSource != element || e.Key is not (Key.Enter or Key.Space)) return;
                e.Handled = true;
                onClick();
            };
        }

        // ---------- Compact list row ----------

        public static FrameworkElement CompactRow(Movie m, bool showPriorityDetails)
        {
            var entry = MovieStore.Instance.Entry(m.Id);
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 14), Background = Brushes.Transparent };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.Children.Add(Poster(m, 48, 72, 6));

            var text = new StackPanel { Margin = new Thickness(14, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center };
            var title = Text(m.Title, "Text.CardTitle");
            title.FontSize = 15;
            text.Children.Add(title);
            text.Children.Add(Text(Format.Meta(m), "Text.Meta", new Thickness(0, 3, 0, 0)));
            if (showPriorityDetails)
            {
                var row = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
                var rt = Text(Format.Runtime(m.RuntimeMin), "Text.Meta", new Thickness(0, 0, 10, 0));
                row.Children.Add(rt);
                if (entry?.HighPriority == true)
                {
                    var hp = Text("★ High priority", "Text.Rating");
                    hp.FontSize = 12;
                    row.Children.Add(hp);
                }
                text.Children.Add(row);
            }
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            var hpOn = entry?.HighPriority == true;
            var star = IconButton(hpOn ? Glyphs.StarFill : Glyphs.Star,
                hpOn ? $"Remove high priority from {m.Title}" : $"Mark {m.Title} as high priority",
                () => MovieActions.TogglePriority(m), small: true, active: hpOn);
            star.Foreground = Ui.Brush("Gold");
            star.Tag = $"star:{m.Id}";
            Grid.SetColumn(star, 2);
            grid.Children.Add(star);

            MakeClickable(grid, $"{m.Title}, {Format.Meta(m)}. Open details", () => Shell.Details(m));
            return grid;
        }
    }
}
