namespace FinalYearProject
{
    public class CatalogPage : ContentPage
    {
        private readonly IReadOnlyList<CatalogItem> _items;
        private readonly VerticalStackLayout _list = new() { Spacing = 10 };
        private readonly SearchBar _search = new() { Placeholder = "Search..." };

        protected CatalogPage(string title, string subtitle, string glyph, IReadOnlyList<CatalogItem> items)
        {
            Title = title;
            _items = items;
            _search.Placeholder = $"Search {title.ToLowerInvariant()}...";
            _search.TextChanged += (_, _) => Render();
            Content = UI.Body(UI.Hero(title, subtitle, glyph), _search, _list);
            Render();
        }

        private void Render()
        {
            _list.Children.Clear();
            var query = _search.Text?.Trim() ?? string.Empty;
            var matches = _items.Where(i => query.Length == 0 ||
                i.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                i.Season.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matches.Count == 0)
            {
                _list.Add(UI.Empty(Icons.Eco, $"Nothing matches \"{query}\"."));
                return;
            }

            foreach (var item in matches)
            {
                var row = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                    ColumnSpacing = 14
                };
                var initial = new Label { Text = item.Initial, Style = UI.Style("IconGlyph"), FontFamily = "PoppinsSemiBold", FontSize = 20 };
                var badge = new Border { Style = UI.Style("IconBadge"), Content = initial };
                row.Add(badge, 0);
                row.Add(new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = item.Name, Style = UI.Style("SectionTitle") },
                        UI.Text(item.Summary, "Caption")
                    }
                }, 1);
                row.Add(new Label { Text = Icons.Arrow, FontFamily = Icons.Font, FontSize = 22, TextColor = UI.Color("Green4"), VerticalOptions = LayoutOptions.Center }, 2);

                var card = UI.Card(row, 14);
                var captured = item;
                card.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () => await Navigation.PushAsync(new CatalogDetailPage(captured)))
                });
                _list.Add(card);
            }
        }
    }

    public sealed class FruitsPage : CatalogPage
    {
        public FruitsPage() : base("Fruits", "Browse fruit with growing guides and add them to your garden.", Icons.Food, PlantCatalog.Fruits) { }
    }

    public sealed class VegetablesPage : CatalogPage
    {
        public VegetablesPage() : base("Vegetables", "Browse vegetables with growing guides and add them to your garden.", Icons.Grass, PlantCatalog.Vegetables) { }
    }

    public sealed class CatalogDetailPage : ContentPage
    {
        public CatalogDetailPage(CatalogItem item)
        {
            Title = item.Name;

            View Fact(string glyph, string label, string value)
            {
                var grid = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                    ColumnSpacing = 14
                };
                grid.Add(UI.Badge(glyph, "Green3", 40), 0);
                grid.Add(new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = { UI.Text(label, "Caption"), new Label { Text = value, FontFamily = "PoppinsSemiBold" } }
                }, 1);
                return grid;
            }

            var facts = new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    Fact(Icons.Sun, "Sunlight", item.Sun),
                    Fact(Icons.Water, "Watering", $"{item.Litres} L every {item.IntervalDays} days"),
                    Fact(Icons.Spa, "Soil", item.Soil),
                    Fact(Icons.Timeline, "Best season", item.Season),
                    Fact(Icons.Eco, "Plant family", item.Family)
                }
            };

            var tip = UI.Card(new VerticalStackLayout
            {
                Spacing = 6,
                Children = { new Label { Text = "Growing tip", Style = UI.Style("SectionTitle") }, new Label { Text = item.Tip } }
            });

            var add = new Button { Text = "Add to my plants" };
            add.Clicked += async (_, _) =>
            {
                App.Database.AddPlant(new DBPlants
                {
                    UserId = App.CurrentUserId,
                    PlantName = item.Name,
                    Season = item.Season,
                    WaterPerLiters = item.Litres,
                    WateringIntervalDays = item.IntervalDays
                });
                await DisplayAlert("Added", $"{item.Name} is now in My plants and your first watering is scheduled.", "OK");
                await Navigation.PopAsync();
            };

            Content = UI.Body(UI.Hero(item.Name, $"{item.Kind} · {item.Season}", item.Kind == "Fruit" ? Icons.Food : Icons.Grass),
                UI.Card(facts), tip, add);
        }
    }
}
