using System.Windows;
using System.Windows.Controls;
using CineTrack.Controls;
using CineTrack.Services;

namespace CineTrack
{
    public partial class AboutPage : CinePage
    {
        public AboutPage()
        {
            InitializeComponent();
            Root.Children.Add(Components.PageHeader("About", "About CineTrack",
                "Letterboxd meets a classic movie theater — a personal movie companion."));

            var body = new StackPanel();
            body.Children.Add(Components.Text("How it works", "Text.Panel", new Thickness(0, 0, 0, 18)));

            var steps = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
            var names = new[] { "Discover", "Add to Watchlist", "Mark Watched", "Rate", "See Stats" };
            for (var i = 0; i < names.Length; i++)
            {
                var chip = Components.Tag($"{i + 1}  {names[i]}", i == 0 ? Components.TagKind.Watched : Components.TagKind.Unwatched);
                chip.Padding = new Thickness(14, 7, 14, 7);
                chip.Margin = new Thickness(0, 0, 8, 10);
                steps.Children.Add(chip);
            }
            body.Children.Add(steps);

            foreach (var p in new[]
            {
                "Browse movies by genre or search by title, director, actor, or phrases like “Sci-Fi under 2 hours”. Save anything that catches your eye to your watchlist and star your must-sees.",
                "When you've watched something, mark it and give it a rating. Your stats — hours watched, top genre, time to clear your list — update instantly.",
                "There are no accounts. Everything you save stays on this device.",
            })
            {
                var t = Components.Text(p, "Text.Body", new Thickness(0, 0, 0, 16));
                t.MaxWidth = 720;
                t.HorizontalAlignment = HorizontalAlignment.Left;
                body.Children.Add(t);
            }

            var go = Components.ActionButton("Start discovering", "Btn.Primary", null, () => Shell.Discover());
            go.HorizontalAlignment = HorizontalAlignment.Left;
            go.Margin = new Thickness(0, 8, 0, 0);
            body.Children.Add(go);

            Root.Children.Add(new Border
            {
                Style = Ui.Res<Style>("Panel"),
                Padding = new Thickness(32),
                Margin = new Thickness(0, 40, 0, 0),
                Child = body,
            });
        }
    }
}
