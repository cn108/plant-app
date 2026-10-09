namespace FinalYearProject
{
    // Offline, rule-based assistant: answers from the built-in plant catalogue and care knowledge.
    public sealed class ChatPage : ContentPage
    {
        private readonly VerticalStackLayout _messages = new() { Spacing = 10, Padding = new Thickness(16, 12) };
        private readonly ScrollView _scroller;
        private readonly Entry _input = new() { Placeholder = "Ask about watering, pests, a plant...", HorizontalOptions = LayoutOptions.Fill };

        public ChatPage()
        {
            Title = "Live Chat Support";
            _scroller = new ScrollView { Content = _messages };

            var send = new Button { Text = Icons.Send, FontFamily = Icons.Font, FontSize = 22, WidthRequest = 52, Padding = 0 };
            send.Clicked += (_, _) => Send(_input.Text);
            _input.Completed += (_, _) => Send(_input.Text);

            var inputBar = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                ColumnSpacing = 8,
                Padding = 12,
                BackgroundColor = Colors.White
            };
            inputBar.Add(_input, 0);
            inputBar.Add(send, 1);

            var chips = new HorizontalStackLayout { Spacing = 8, Padding = new Thickness(12, 8) };
            foreach (var suggestion in new[] { "How often should I water?", "Yellow leaves", "Pests", "Tomatoes", "When is frost a risk?" })
            {
                var chip = new Button { Text = suggestion, FontSize = 12, HeightRequest = 34, MinimumHeightRequest = 34, Padding = new Thickness(12, 0) };
                if (UI.Style("SecondaryButton") is { } s) chip.Style = s;
                chip.Clicked += (_, _) => Send(suggestion);
                chips.Add(chip);
            }

            var grid = new Grid
            {
                RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
                BackgroundColor = UI.Color("Background")
            };
            grid.Add(new Border { Style = UI.Style("HeroBanner"), Margin = 12, Content = new Label { Text = "Plant Assistant · ask me anything about your garden", TextColor = Colors.White, FontFamily = "PoppinsSemiBold" } }, 0, 0);
            grid.Add(_scroller, 0, 1);
            grid.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = chips }, 0, 2);
            grid.Add(inputBar, 0, 3);
            Content = grid;

            AddBubble("Hi! I'm your Plant Assistant. Ask about watering, pests, light, frost, or any fruit or vegetable in the catalogue.", false);
        }

        private void Send(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            _input.Text = string.Empty;
            AddBubble(text.Trim(), true);
            AddBubble(Reply(text), false);
        }

        private void AddBubble(string text, bool fromUser)
        {
            var bubble = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                BackgroundColor = fromUser ? UI.Color("Green3") : Colors.White,
                Padding = new Thickness(14, 10),
                MaximumWidthRequest = 520,
                HorizontalOptions = fromUser ? LayoutOptions.End : LayoutOptions.Start,
                Content = new Label { Text = text, TextColor = fromUser ? Colors.White : UI.Color("TextColor"), LineBreakMode = LineBreakMode.WordWrap }
            };
            _messages.Add(bubble);
            Dispatcher.Dispatch(async () =>
            {
                await Task.Delay(50);
                await _scroller.ScrollToAsync(bubble, ScrollToPosition.End, true);
            });
        }

        private static string Reply(string question)
        {
            var q = question.ToLowerInvariant();

            var plant = PlantCatalog.All.FirstOrDefault(p => q.Contains(p.Name.ToLowerInvariant().TrimEnd('s')));
            if (plant is not null)
            {
                return $"{plant.Name}: {plant.Sun.ToLowerInvariant()}, {plant.Soil.ToLowerInvariant()} soil. Water about {plant.Litres} L every {plant.IntervalDays} days. {plant.Tip}";
            }

            if (Has(q, "yellow", "wilt", "droop")) return "Yellow leaves are usually overwatering or poor drainage. Let the soil dry out, check the pot drains, and feed lightly if growth is pale. Drooping with dry soil means it needs water.";
            if (Has(q, "pest", "bug", "aphid", "insect", "mite")) return "Inspect under leaves. For aphids and mites, rinse with water, then spray with diluted insecticidal soap every few days. Remove badly affected leaves and isolate infected plants.";
            if (Has(q, "water", "irrigat")) return "Check the top 2-3 cm of soil: if dry, water deeply at the base. Most vegetables need water every 2-4 days in summer; use the Weather page to skip watering after rain.";
            if (Has(q, "frost", "cold", "winter")) return "Protect tender plants when temperatures drop below 3 °C: cover with fleece, move pots indoors, and water sparingly.";
            if (Has(q, "light", "sun", "shade")) return "Most fruit and vegetables want 6+ hours of direct sun. Leafy greens like lettuce and spinach tolerate part shade, especially in hot weather.";
            if (Has(q, "fertil", "feed", "compost")) return "Feed during the growing season only. Use compost at planting, then a balanced feed every 2-4 weeks for fruiting crops. Avoid feeding in winter.";
            if (Has(q, "rotat")) return "Rotate crops by family: legumes, then leafy brassicas, then roots, then fruiting crops. Try the Crop Rotation Planner for advice on your beds.";
            if (Has(q, "when", "plant", "sow", "season")) return "Cool-season crops (lettuce, peas, spinach) go in during spring or autumn; warm-season crops (tomatoes, peppers, beans) after the last frost. Open any plant in Fruits or Vegetables for its best season.";
            if (Has(q, "hello", "hi ", "hey") || q is "hi" or "hey") return "Hello! What would you like to know about your plants today?";
            if (Has(q, "thank")) return "You're welcome. Happy growing!";

            return "I can help with watering, light, pests, feeding, frost and the plants in our Fruits and Vegetables lists. Try asking, for example, \"How do I grow tomatoes?\"";
        }

        private static bool Has(string text, params string[] words) => words.Any(text.Contains);
    }
}
