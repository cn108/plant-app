namespace FinalYearProject
{
    // Small code-side helpers so the code-built pages share the app's visual language.
    public static class UI
    {
        public static Color Color(string key) =>
            Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color c ? c : Colors.Black;

        public static Style? Style(string key) =>
            Application.Current?.Resources.TryGetValue(key, out var value) == true ? value as Style : null;

        public static Label Text(string text, string? style = null, Color? color = null, double? size = null, bool wrap = true)
        {
            var label = new Label { Text = text, LineBreakMode = wrap ? LineBreakMode.WordWrap : LineBreakMode.TailTruncation };
            if (style is not null && Style(style) is { } s)
            {
                label.Style = s;
            }

            if (color is not null) label.TextColor = color;
            if (size is not null) label.FontSize = size.Value;
            return label;
        }

        public static Border Badge(string glyph, string colorKey = "Green2", double size = 44)
        {
            var badge = new Border
            {
                Style = Style("IconBadge"),
                WidthRequest = size,
                HeightRequest = size,
                VerticalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = glyph,
                    Style = Style("IconGlyph"),
                    TextColor = Color(colorKey),
                    FontSize = size * 0.5
                }
            };
            return badge;
        }

        public static Border Card(View content, double padding = 16)
        {
            var card = new Border { Style = Style("Card"), Content = content };
            card.Padding = padding;
            return card;
        }

        public static Border Hero(string title, string subtitle, string glyph)
        {
            var icon = new Label
            {
                Text = glyph,
                FontFamily = Icons.Font,
                FontSize = 36,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            };
            var texts = new VerticalStackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = title, FontFamily = "PoppinsSemiBold", FontSize = 24, TextColor = Colors.White },
                    new Label { Text = subtitle, FontSize = 13, TextColor = Colors.White, LineBreakMode = LineBreakMode.WordWrap }
                }
            };
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 14 };
            grid.Add(icon, 0);
            grid.Add(texts, 1);
            return new Border { Style = Style("HeroBanner"), Content = grid };
        }

        public static Border Empty(string glyph, string message, string? actionText = null, Func<Task>? action = null)
        {
            var stack = new VerticalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center, Padding = new Thickness(8, 24) };
            stack.Add(Badge(glyph, "Green4", 64));
            stack.Add(new Label { Text = message, HorizontalTextAlignment = TextAlignment.Center, TextColor = Color("Muted") });
            if (actionText is not null && action is not null)
            {
                var button = new Button { Text = actionText, HorizontalOptions = LayoutOptions.Center };
                button.Clicked += async (_, _) => await action();
                stack.Add(button);
            }

            return Card(stack);
        }

        public static Button Secondary(string text, Func<Task> action)
        {
            var button = new Button { Text = text };
            if (Style("SecondaryButton") is { } s) button.Style = s;
            button.Clicked += async (_, _) => await action();
            return button;
        }

        public static Entry StyledEntry(string placeholder, Keyboard? keyboard = null, bool password = false) =>
            new() { Placeholder = placeholder, Keyboard = keyboard ?? Keyboard.Default, IsPassword = password, TextColor = Color("TextColor") };

        // Standard scrolling page body: hero + content, centred with a readable maximum width.
        public static ScrollView Body(Border hero, params View[] children)
        {
            var stack = new VerticalStackLayout { Spacing = 16, Padding = 20, MaximumWidthRequest = 860, HorizontalOptions = LayoutOptions.Center };
            stack.Add(hero);
            foreach (var child in children) stack.Add(child);
            return new ScrollView { Content = stack, BackgroundColor = Color("Background") };
        }

        public static Task Push(Page page) =>
            Application.Current?.MainPage?.Navigation.PushAsync(page) ?? Task.CompletedTask;

        // Moves to a flyout page by title when inside the Shell.
        public static void GoTo(string title)
        {
            var item = Shell.Current?.Items.FirstOrDefault(i => string.Equals(i.Title, title, StringComparison.OrdinalIgnoreCase));
            if (item is not null)
            {
                Shell.Current!.CurrentItem = item;
            }
        }

        public static string Pretty(DateTime due)
        {
            var days = (due.Date - DateTime.Today).Days;
            return days switch
            {
                < -1 => $"{-days} days overdue",
                -1 => "Yesterday",
                0 => "Today",
                1 => "Tomorrow",
                _ => $"In {days} days · {due:dd MMM}"
            };
        }
    }
}
