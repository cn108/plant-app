namespace FinalYearProject;

public partial class DashboardPage : ContentPage
{
    private static readonly (string Title, string Label, string Glyph)[] QuickActions =
    [
        ("Scan Plants", "Scan", Icons.Camera),
        ("Add Plants", "Add plant", Icons.Add),
        ("View Added Plants", "My plants", Icons.List),
        ("Plant Care Timeline", "Timeline", Icons.Timeline),
        ("Forum", "Community", Icons.Forum),
        ("Plant Care Tips", "Care tips", Icons.Tips),
        ("Crop Rotation Planner", "Rotation", Icons.Rotate),
        ("Live Chat Support", "Support", Icons.Chat),
    ];

    private bool _animated;

    public DashboardPage()
    {
        InitializeComponent();
        BuildQuickActions();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var hour = DateTime.Now.Hour;
        var greeting = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
        var name = App.CurrentUserName;
        GreetingLabel.Text = string.IsNullOrWhiteSpace(name) ? greeting : $"{greeting}, {name}";

        if (_animated) return;
        _animated = true;

        // Fade and slide each section in for a smoother first impression.
        var delay = 0;
        foreach (var child in Content.Children.OfType<VisualElement>())
        {
            child.Opacity = 0;
            child.TranslationY = 16;
            _ = Task.Delay(delay).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.WhenAll(child.FadeTo(1, 320, Easing.CubicOut), child.TranslateTo(0, 0, 320, Easing.CubicOut));
            }));
            delay += 60;
        }
        await Task.CompletedTask;
    }

    private void BuildQuickActions()
    {
        foreach (var (title, label, glyph) in QuickActions)
        {
            var tile = new Border
            {
                Style = (Style)Application.Current!.Resources["Tile"],
                WidthRequest = 150,
                Margin = new Thickness(0, 0, 12, 12),
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Border
                        {
                            Style = (Style)Application.Current!.Resources["IconBadge"],
                            HorizontalOptions = LayoutOptions.Center,
                            Content = new Label { Text = glyph, Style = (Style)Application.Current!.Resources["IconGlyph"] }
                        },
                        new Label
                        {
                            Text = label,
                            FontFamily = "PoppinsSemiBold",
                            FontSize = 14,
                            HorizontalTextAlignment = TextAlignment.Center
                        }
                    }
                }
            };

            var target = title;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) =>
            {
                await tile.ScaleTo(0.95, 70);
                await tile.ScaleTo(1, 70);
                OpenSection(target);
            };
            tile.GestureRecognizers.Add(tap);
            TileGrid.Children.Add(tile);
        }
    }

    private static void OpenSection(string title)
    {
        var item = Shell.Current.Items.FirstOrDefault(i => i.Title == title);
        if (item is not null) Shell.Current.CurrentItem = item;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) =>
        SearchResults.IsVisible = !string.IsNullOrWhiteSpace(e.NewTextValue);
}
