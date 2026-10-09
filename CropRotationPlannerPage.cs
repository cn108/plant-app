namespace FinalYearProject
{
    // Tap a crop to select it, then tap a bed to plant it. Saved beds drive rotation advice.
    public sealed class CropRotationPlannerPage : ContentPage
    {
        private const int BedCount = 4;
        private readonly string?[] _beds = new string?[BedCount];
        private readonly string?[] _previous = new string?[BedCount];
        private readonly Grid _bedGrid = new() { RowSpacing = 12, ColumnSpacing = 12 };
        private readonly HorizontalStackLayout _cropRow = new() { Spacing = 8 };
        private readonly VerticalStackLayout _advice = new() { Spacing = 6 };
        private string? _selected;

        public CropRotationPlannerPage()
        {
            Title = "Crop Rotation Planner";
            _bedGrid.RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(GridLength.Star) };
            _bedGrid.ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) };

            foreach (var bed in App.Database.GetAllBeds().Where(b => b.Position is >= 1 and <= BedCount))
            {
                _beds[bed.Position - 1] = bed.CurrentCrop;
                _previous[bed.Position - 1] = Preferences.Get($"bed_prev_{bed.Position}", string.Empty) is { Length: > 0 } p ? p : null;
            }

            var save = new Button { Text = "Save plan" };
            save.Clicked += async (_, _) => await SaveAsync();

            var clear = UI.Secondary("Clear all beds", async () =>
            {
                Array.Clear(_beds);
                Render();
                await Task.CompletedTask;
            });

            Content = UI.Body(
                UI.Hero("Crop rotation", "Pick a crop, then tap a bed. Rotating plant families keeps soil healthy.", Icons.Rotate),
                UI.Card(new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label { Text = "1. Choose a crop", Style = UI.Style("SectionTitle") },
                        new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = _cropRow }
                    }
                }),
                UI.Card(new VerticalStackLayout
                {
                    Spacing = 8,
                    Children = { new Label { Text = "2. Tap a bed to plant it", Style = UI.Style("SectionTitle") }, _bedGrid }
                }),
                UI.Card(new VerticalStackLayout
                {
                    Spacing = 8,
                    Children = { new Label { Text = "Rotation advice", Style = UI.Style("SectionTitle") }, _advice }
                }),
                save, clear);

            Render();
        }

        private void Render()
        {
            _cropRow.Children.Clear();
            foreach (var crop in PlantCatalog.Vegetables)
            {
                var active = crop.Name == _selected;
                var chip = new Button
                {
                    Text = crop.Name,
                    BackgroundColor = active ? UI.Color("Green1") : UI.Color("Green3"),
                    FontSize = 13,
                    MinimumHeightRequest = 38,
                    HeightRequest = 38,
                    Padding = new Thickness(14, 0)
                };
                var name = crop.Name;
                chip.Clicked += (_, _) =>
                {
                    _selected = _selected == name ? null : name;
                    Render();
                };
                _cropRow.Add(chip);
            }

            _bedGrid.Children.Clear();
            for (var i = 0; i < BedCount; i++)
            {
                var index = i;
                var crop = _beds[i];
                var tile = new Border
                {
                    Style = UI.Style("Tile"),
                    HeightRequest = 96,
                    BackgroundColor = crop is null ? UI.Color("Background") : UI.Color("Green3"),
                    Content = new VerticalStackLayout
                    {
                        VerticalOptions = LayoutOptions.Center,
                        Spacing = 2,
                        Children =
                        {
                            new Label { Text = $"Bed {i + 1}", FontSize = 12, TextColor = crop is null ? UI.Color("Muted") : Colors.White, HorizontalOptions = LayoutOptions.Center },
                            new Label { Text = crop ?? "Empty", FontFamily = "PoppinsSemiBold", FontSize = 17, TextColor = crop is null ? UI.Color("Green3") : Colors.White, HorizontalOptions = LayoutOptions.Center }
                        }
                    }
                };
                tile.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() =>
                    {
                        // Tapping with no crop selected clears the bed.
                        _beds[index] = _selected;
                        Render();
                    })
                });
                _bedGrid.Add(tile, i % 2, i / 2);
            }

            _advice.Children.Clear();
            var any = false;
            for (var i = 0; i < BedCount; i++)
            {
                if (_beds[i] is null) continue;
                any = true;
                var tip = _previous[i] is null
                    ? $"Bed {i + 1}: {_beds[i]} planted. Save the plan, and next season you will get advice on what follows."
                    : $"Bed {i + 1}: {PlantCatalog.RotationAdvice(_previous[i], _beds[i])}";
                _advice.Add(new Label { Text = tip, FontSize = 14 });
            }

            if (!any)
            {
                _advice.Add(UI.Text("Plant a crop in a bed to see advice.", "Caption"));
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                var previous = App.Database.GetAllBeds().ToDictionary(b => b.Position, b => b.CurrentCrop);
                for (var i = 0; i < BedCount; i++)
                {
                    var position = i + 1;
                    previous.TryGetValue(position, out var before);
                    if (!string.Equals(before, _beds[i], StringComparison.Ordinal))
                    {
                        Preferences.Set($"bed_prev_{position}", before ?? string.Empty);
                        _previous[i] = string.IsNullOrEmpty(before) ? null : before;
                    }

                    App.Database.SaveBedConfiguration(new CropBed { Position = position, CurrentCrop = _beds[i] });
                }

                Render();
                await DisplayAlert("Saved", "Your crop rotation plan has been saved.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Saving crop rotation failed: {ex}");
                await DisplayAlert("Error", "Failed to save the plan.", "OK");
            }
        }
    }
}
