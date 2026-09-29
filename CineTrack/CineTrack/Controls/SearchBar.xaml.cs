using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CineTrack.Services;

namespace CineTrack.Controls
{
    /// <summary>Search bar (spec 5.2). Compact in the header, large on Home and the search page.</summary>
    public partial class SearchBar : UserControl
    {
        public SearchBar()
        {
            InitializeComponent();
            Ui.SetGlyph(FiltersButton, Glyphs.Filter);
            IsKeyboardFocusWithinChanged += (_, _) =>
                Pill.BorderBrush = Ui.Brush(IsKeyboardFocusWithin ? "Gold" : "Cream");
            SizeChanged += (_, _) => UpdateFiltersVisibility();
        }

        bool _showFilters;

        // The Filters pill is dropped on phone widths so the input keeps room to type.
        void UpdateFiltersVisibility() =>
            FiltersButton.Visibility = _showFilters && ActualWidth >= 520 ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>Raised on submit. When nobody handles it, the search page opens.</summary>
        public event EventHandler<string>? Submitted;

        public string Text
        {
            get => Input.Text;
            set => Input.Text = value;
        }

        public void SetLarge(bool large, bool showFilters)
        {
            Pill.Height = large ? 64 : 48;
            Pill.Padding = new Thickness(large ? 7 : 5);
            Input.FontSize = Placeholder.FontSize = large ? 17 : 15;
            SearchIcon.FontSize = large ? 18 : 15;
            SearchIcon.Margin = new Thickness(large ? 20 : 14, 0, 12, 0);
            SubmitButton.FontSize = large ? 15 : 14;
            SubmitButton.Padding = new Thickness(large ? 28 : 20, 0, large ? 28 : 20, 0);
            _showFilters = showFilters;
            UpdateFiltersVisibility();
        }

        public void FocusInput()
        {
            Input.Focus();
            Input.SelectAll();
        }

        void Input_TextChanged(object sender, TextChangedEventArgs e) =>
            Placeholder.Visibility = Input.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

        void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Submit();
            }
            else if (e.Key == Key.Escape)
            {
                Input.Clear();
            }
        }

        void SubmitButton_Click(object sender, RoutedEventArgs e) => Submit();

        void FiltersButton_Click(object sender, RoutedEventArgs e) => Shell.Discover();

        void Submit()
        {
            var q = Input.Text.Trim();
            if (Submitted != null) Submitted.Invoke(this, q);
            else if (q.Length > 0) Shell.Search(q);
            else FocusInput();
        }
    }
}
