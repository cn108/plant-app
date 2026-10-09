using System.Net.Mail;
using FinalYearProject.Models;

namespace FinalYearProject
{
    public sealed class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            Title = "Welcome";
            NavigationPage.SetHasNavigationBar(this, false);

            var signIn = new Button
            {
                Text = "Sign in",
                BackgroundColor = Colors.White,
                TextColor = UI.Color("Green1"),
                MaximumWidthRequest = 360,
                HorizontalOptions = LayoutOptions.Fill
            };
            signIn.Clicked += async (_, _) => await Navigation.PushAsync(new LoginPage());

            var register = new Button
            {
                Text = "Create an account",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.White,
                BorderColor = Colors.White,
                BorderWidth = 1.5,
                MaximumWidthRequest = 360,
                HorizontalOptions = LayoutOptions.Fill
            };
            register.Clicked += async (_, _) => await Navigation.PushAsync(new RegistrationPage());

            Content = new Grid
            {
                Background = new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(UI.Color("Green1"), 0f),
                        new GradientStop(UI.Color("Green3"), 0.7f),
                        new GradientStop(UI.Color("Green4"), 1f)
                    }, new Point(0, 0), new Point(1, 1)),
                Children =
                {
                    new VerticalStackLayout
                    {
                        Padding = 32,
                        Spacing = 14,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Border
                            {
                                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 48 },
                                StrokeThickness = 0,
                                BackgroundColor = Color.FromRgba(255, 255, 255, 40),
                                WidthRequest = 96,
                                HeightRequest = 96,
                                HorizontalOptions = LayoutOptions.Center,
                                Content = new Label
                                {
                                    Text = Icons.Eco,
                                    FontFamily = Icons.Font,
                                    FontSize = 54,
                                    TextColor = Colors.White,
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center
                                }
                            },
                            new Label { Text = "Plant Companion", FontFamily = "PoppinsSemiBold", FontSize = 34, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center },
                            new Label
                            {
                                Text = "Identify plants, plan your beds and never miss a watering.",
                                FontSize = 16,
                                TextColor = Colors.White,
                                HorizontalTextAlignment = TextAlignment.Center,
                                MaximumWidthRequest = 420
                            },
                            new BoxView { HeightRequest = 12, Color = Colors.Transparent },
                            signIn,
                            register
                        }
                    }
                }
            };
        }
    }

    public sealed class RegistrationPage : ContentPage
    {
        private readonly Entry _usernameEntry = UI.StyledEntry("Name");
        private readonly Entry _emailEntry = UI.StyledEntry("Email", Keyboard.Email);
        private readonly Entry _passwordEntry = UI.StyledEntry("Password (at least 8 characters)", password: true);
        private readonly Label _statusLabel = new() { TextColor = Colors.DarkRed, IsVisible = false };
        private readonly Button _registerButton = new() { Text = "Create account" };

        public RegistrationPage()
        {
            Title = "Create account";
            _registerButton.Clicked += async (_, _) => await RegisterAsync();
            _passwordEntry.Completed += async (_, _) => await RegisterAsync();

            var showPassword = new CheckBox { Color = UI.Color("Green3") };
            showPassword.CheckedChanged += (_, e) => _passwordEntry.IsPassword = !e.Value;
            var showRow = new HorizontalStackLayout
            {
                Spacing = 4,
                Children = { showPassword, new Label { Text = "Show password", VerticalOptions = LayoutOptions.Center } }
            };

            var form = new VerticalStackLayout
            {
                Spacing = 12,
                Children = { _usernameEntry, _emailEntry, _passwordEntry, showRow, _statusLabel, _registerButton }
            };

            Content = UI.Body(UI.Hero("Join Plant Companion", "Create an account to save plants, reminders and forum posts.", Icons.Person), UI.Card(form, 20));
        }

        private void ShowError(string message)
        {
            _statusLabel.Text = message;
            _statusLabel.IsVisible = true;
        }

        private async Task RegisterAsync()
        {
            var username = _usernameEntry.Text?.Trim();
            var email = _emailEntry.Text?.Trim();
            var password = _passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowError("Name, email, and password are required.");
                return;
            }

            try
            {
                _ = new MailAddress(email);
            }
            catch (FormatException)
            {
                ShowError("Enter a valid email address.");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Choose a password with at least 8 characters.");
                return;
            }

            _registerButton.IsEnabled = false;
            try
            {
                var online = await ApiClient.RegisterAsync(username, email, password);
                if (online.Ok && online.Value is not null)
                {
                    SignIn(online.Value.UserId, email, username);
                    return;
                }

                if (!online.Unreachable)
                {
                    ShowError(online.Error ?? "Unable to create the account.");
                    return;
                }

                var user = new DBUsers
                {
                    Username = username,
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password)
                };
                App.Database.AddUser(user);
                SignIn(user.UserId, user.Email, username);
            }
            catch (InvalidOperationException)
            {
                ShowError("An account with this email already exists.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Account registration failed: {ex}");
                ShowError("Unable to create the account right now. Please try again.");
            }
            finally
            {
                _registerButton.IsEnabled = true;
            }
        }

        private static void SignIn(int userId, string email, string username)
        {
            App.SetCurrentUser(userId, username);
            Preferences.Set("user_id", userId);
            Preferences.Set("user_email", email);
            Preferences.Set("user_username", username);
            Application.Current!.MainPage = new AppShell();
        }
    }

    public sealed class WeatherPage : ContentPage
    {
        private static readonly (string Name, double Lat, double Lon)[] Cities =
        {
            ("London", 51.5074, -0.1278), ("Lagos", 6.5244, 3.3792), ("Abuja", 9.0765, 7.3986),
            ("New York", 40.7128, -74.0060), ("Nairobi", -1.2921, 36.8219), ("Mumbai", 19.0760, 72.8777),
            ("Sydney", -33.8688, 151.2093), ("Toronto", 43.6532, -79.3832)
        };

        private readonly Picker _cityPicker = new() { Title = "Choose a city" };
        private readonly Label _temperature = new() { FontFamily = "PoppinsSemiBold", FontSize = 56, TextColor = Colors.White };
        private readonly Label _summary = new() { TextColor = Colors.White, FontSize = 14 };
        private readonly Label _humidity = new() { FontFamily = "PoppinsSemiBold", FontSize = 18 };
        private readonly Label _wind = new() { FontFamily = "PoppinsSemiBold", FontSize = 18 };
        private readonly Label _rain = new() { FontFamily = "PoppinsSemiBold", FontSize = 18 };
        private readonly Label _advice = new() { LineBreakMode = LineBreakMode.WordWrap };
        private readonly ActivityIndicator _busy = new() { Color = Colors.White };
        private string _place = Cities[0].Name;
        private double _lat = Cities[0].Lat;
        private double _lon = Cities[0].Lon;

        public WeatherPage()
        {
            Title = "Weather";
            _cityPicker.ItemsSource = Cities.Select(c => c.Name).ToList();
            _cityPicker.SelectedIndex = Preferences.Get("weather_city", 0) is var i && i >= 0 && i < Cities.Length ? i : 0;
            ApplyCity();
            _cityPicker.SelectedIndexChanged += async (_, _) =>
            {
                if (_cityPicker.SelectedIndex < 0) return;
                Preferences.Set("weather_city", _cityPicker.SelectedIndex);
                ApplyCity();
                await LoadWeatherAsync();
            };

            var hero = new Border
            {
                Style = UI.Style("HeroBanner"),
                Content = new VerticalStackLayout
                {
                    Spacing = 2,
                    Children =
                    {
                        new Label { Text = "Current weather", TextColor = Colors.White, FontSize = 13 },
                        _temperature,
                        _summary,
                        _busy
                    }
                }
            };

            Border Stat(string glyph, string caption, Label value)
            {
                return UI.Card(new VerticalStackLayout
                {
                    Spacing = 4,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = glyph, FontFamily = Icons.Font, FontSize = 26, TextColor = UI.Color("Green4"), HorizontalOptions = LayoutOptions.Center },
                        value,
                        UI.Text(caption, "Caption")
                    }
                }, 12);
            }

            var stats = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
            stats.Add(Stat(Icons.Water, "Humidity", _humidity), 0);
            stats.Add(Stat(Icons.Cloud, "Wind", _wind), 1);
            stats.Add(Stat(Icons.Thermo, "Rain", _rain), 2);

            var adviceCard = UI.Card(new VerticalStackLayout
            {
                Spacing = 6,
                Children = { new Label { Text = "Gardening advice", Style = UI.Style("SectionTitle") }, _advice }
            });

            var locate = UI.Secondary("Use my location", UseMyLocationAsync);
            var refresh = UI.Secondary("Refresh", LoadWeatherAsync);

            Content = UI.Body(UI.Hero("Weather", "Plan your watering with live conditions.", Icons.Sun),
                _cityPicker, hero, stats, adviceCard, locate, refresh);
        }

        private void ApplyCity()
        {
            var city = Cities[Math.Max(0, _cityPicker.SelectedIndex)];
            (_place, _lat, _lon) = city;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadWeatherAsync();
        }

        private async Task UseMyLocationAsync()
        {
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(8)));
                if (location is null)
                {
                    await DisplayAlert("Location", "Your location could not be determined. Choose a city instead.", "OK");
                    return;
                }

                (_place, _lat, _lon) = ("Your location", location.Latitude, location.Longitude);
                await LoadWeatherAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location lookup failed: {ex}");
                await DisplayAlert("Location", "Location is unavailable or permission was denied. Choose a city instead.", "OK");
            }
        }

        private async Task LoadWeatherAsync()
        {
            _busy.IsRunning = true;
            _summary.Text = _place;
            try
            {
                var weather = await new WeatherService().GetWeatherAsync(_lat, _lon);
                _temperature.Text = $"{weather.Temperature:0.#} °C";
                _humidity.Text = weather.Humidity is { } h ? $"{h:0}%" : "–";
                _wind.Text = weather.WindKph is { } w ? $"{w:0} km/h" : "–";
                _rain.Text = weather.RainMm is { } r ? $"{r:0.#} mm" : "–";
                _advice.Text = weather.Advice;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loading current weather failed: {ex}");
                _temperature.Text = "--";
                _advice.Text = "Weather is unavailable. Check your connection and try again.";
            }
            finally
            {
                _busy.IsRunning = false;
            }
        }
    }

    public sealed class ViewPlantsPage : ContentPage
    {
        private readonly VerticalStackLayout _list = new() { Spacing = 12 };

        public ViewPlantsPage()
        {
            Title = "View Added Plants";
            var add = new Button { Text = "Add a plant" };
            add.Clicked += async (_, _) => await UI.Push(new AddPlantPage());
            Content = UI.Body(UI.Hero("My plants", "Everything you are growing, with the next watering date.", Icons.Eco), add, _list);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Render();
        }

        private void Render()
        {
            _list.Children.Clear();
            var plants = App.Database.GetPlantsByUser(App.CurrentUserId);
            if (plants.Count == 0)
            {
                _list.Add(UI.Empty(Icons.Eco, "You have not added any plants yet.", "Add your first plant", () => UI.Push(new AddPlantPage())));
                return;
            }

            var tasks = App.Database.GetPlantCareTasksByUser(App.CurrentUserId);
            foreach (var plant in plants)
            {
                var next = tasks.Where(t => t.PlantId == plant.PId).OrderBy(t => t.DueDate).FirstOrDefault();
                View thumb = plant.HasImage
                    ? new Border
                    {
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                        StrokeThickness = 0,
                        WidthRequest = 64,
                        HeightRequest = 64,
                        Content = new Image { Source = ImageSource.FromFile(plant.ImagePath), Aspect = Aspect.AspectFill }
                    }
                    : UI.Badge(Icons.Eco, "Green3", 64);

                var delete = new Button
                {
                    Text = Icons.Close,
                    BackgroundColor = Colors.Transparent,
                    TextColor = UI.Color("Danger"),
                    FontFamily = Icons.Font,
                    FontSize = 22,
                    Padding = 0,
                    MinimumHeightRequest = 40,
                    MinimumWidthRequest = 40,
                    VerticalOptions = LayoutOptions.Center
                };
                var captured = plant;
                delete.Clicked += async (_, _) =>
                {
                    if (await DisplayAlert("Remove plant", $"Remove {captured.PlantName} and its reminders?", "Remove", "Cancel"))
                    {
                        App.Database.DeletePlant(captured.PId);
                        Render();
                    }
                };

                var row = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                    ColumnSpacing = 14
                };
                row.Add(thumb, 0);
                row.Add(new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 2,
                    Children =
                    {
                        new Label { Text = plant.PlantName, Style = UI.Style("SectionTitle") },
                        UI.Text(plant.Summary, "Caption"),
                        new Label
                        {
                            Text = next is null ? "No watering scheduled" : $"Next watering: {UI.Pretty(next.DueDate)}",
                            FontSize = 13,
                            FontFamily = "PoppinsSemiBold",
                            TextColor = next is not null && next.DueDate.Date < DateTime.Today ? UI.Color("Danger") : UI.Color("Green3")
                        }
                    }
                }, 1);
                row.Add(delete, 2);
                _list.Add(UI.Card(row, 14));
            }
        }
    }

    public sealed class PlantCareTipsPage : ContentPage
    {
        private static readonly (string Glyph, string Title, string Body)[] Tips =
        {
            (Icons.Water, "Water smartly", "Check the soil with a finger before watering and water at the base. Waterlogged roots are the most common cause of plant death."),
            (Icons.Sun, "Right light, right place", "Match each plant to its light needs and rotate indoor pots a quarter turn weekly for even growth."),
            (Icons.Spa, "Good drainage", "Use pots with drainage holes and empty standing water from saucers after watering."),
            (Icons.Eco, "Prune and inspect", "Remove dead leaves and check under leaves regularly for pests such as aphids and spider mites."),
            (Icons.Grass, "Feed in season", "Fertilise during the active growing season only and follow the product dose; more is not better."),
            (Icons.Rotate, "Rotate your crops", "Avoid growing the same plant family in the same bed two years running to keep soil healthy."),
            (Icons.Thermo, "Watch the weather", "Skip watering after rain, water early on hot days and protect tender plants from frost.")
        };

        public PlantCareTipsPage()
        {
            Title = "Plant Care Tips";
            var list = new VerticalStackLayout { Spacing = 12 };
            foreach (var (glyph, title, body) in Tips)
            {
                var row = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                    ColumnSpacing = 14
                };
                row.Add(UI.Badge(glyph, "Green3"), 0);
                row.Add(new VerticalStackLayout
                {
                    Spacing = 3,
                    Children = { new Label { Text = title, Style = UI.Style("SectionTitle") }, new Label { Text = body, FontSize = 14, LineBreakMode = LineBreakMode.WordWrap } }
                }, 1);
                list.Add(UI.Card(row, 14));
            }

            Content = UI.Body(UI.Hero("Plant care tips", "Simple habits that keep plants thriving.", Icons.Tips), list);
        }
    }

    public sealed class PlantCareTimelinePage : ContentPage
    {
        private readonly VerticalStackLayout _list = new() { Spacing = 10 };

        public PlantCareTimelinePage()
        {
            Title = "Plant Care Timeline";
            Content = UI.Body(UI.Hero("Care timeline", "Overdue, today and upcoming tasks in one place.", Icons.Timeline), _list);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Render();
        }

        private void Render()
        {
            _list.Children.Clear();
            var tasks = App.Database.GetPlantCareTasksByUser(App.CurrentUserId);
            if (tasks.Count == 0)
            {
                _list.Add(UI.Empty(Icons.Timeline, "No care tasks yet. Add a plant and its watering schedule will appear here.", "Add a plant", () => UI.Push(new AddPlantPage())));
                return;
            }

            AddGroup("Overdue", tasks.Where(t => t.DueDate.Date < DateTime.Today), "Danger");
            AddGroup("Today", tasks.Where(t => t.DueDate.Date == DateTime.Today), "Green3");
            AddGroup("Upcoming", tasks.Where(t => t.DueDate.Date > DateTime.Today), "Green4");
        }

        private void AddGroup(string heading, IEnumerable<PlantCareTask> tasks, string colorKey)
        {
            var items = tasks.OrderBy(t => t.DueDate).ToList();
            if (items.Count == 0) return;

            _list.Add(new Label { Text = $"{heading} ({items.Count})", Style = UI.Style("SectionTitle"), TextColor = UI.Color(colorKey), Margin = new Thickness(0, 6, 0, 0) });
            foreach (var task in items)
            {
                var done = new Button { Text = "Done", HeightRequest = 40, Padding = new Thickness(16, 0), VerticalOptions = LayoutOptions.Center };
                var captured = task;
                done.Clicked += (_, _) =>
                {
                    App.Database.CompletePlantCareTask(captured);
                    Render();
                };

                var row = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                    ColumnSpacing = 14
                };
                row.Add(UI.Badge(Icons.Water, colorKey == "Danger" ? "Danger" : "Green3", 40), 0);
                row.Add(new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = task.PlantName, FontFamily = "PoppinsSemiBold" },
                        UI.Text(task.TaskDescription, "Caption"),
                        new Label { Text = UI.Pretty(task.DueDate), FontSize = 12, TextColor = UI.Color(colorKey) }
                    }
                }, 1);
                row.Add(done, 2);
                _list.Add(UI.Card(row, 12));
            }
        }
    }

    public sealed class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            Title = "Profile";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var name = string.IsNullOrWhiteSpace(App.CurrentUserName) ? "Gardener" : App.CurrentUserName;
            var plants = App.Database.GetPlantsByUser(App.CurrentUserId).Count;
            var tasks = App.Database.GetPlantCareTasksByUser(App.CurrentUserId);
            var due = tasks.Count(t => t.DueDate.Date <= DateTime.Today);

            var avatar = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 40 },
                StrokeThickness = 0,
                BackgroundColor = UI.Color("Green3"),
                WidthRequest = 80,
                HeightRequest = 80,
                HorizontalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = name[..1].ToUpperInvariant(),
                    FontFamily = "PoppinsSemiBold",
                    FontSize = 34,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };

            View Stat(string value, string caption) => new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = value, FontFamily = "PoppinsSemiBold", FontSize = 26, TextColor = UI.Color("Green3"), HorizontalOptions = LayoutOptions.Center },
                    UI.Text(caption, "Caption")
                }
            };

            var stats = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
            stats.Add(Stat(plants.ToString(), "Plants"), 0);
            stats.Add(Stat(tasks.Count.ToString(), "Tasks"), 1);
            stats.Add(Stat(due.ToString(), "Due now"), 2);

            var signOut = new Button { Text = "Sign out", BackgroundColor = UI.Color("Danger") };
            signOut.Clicked += async (_, _) =>
            {
                if (!await DisplayAlert("Sign out", "Sign out of Plant Companion?", "Sign out", "Cancel")) return;
                UserService.LogoutUser();
                Application.Current!.MainPage = new NavigationPage(new WelcomePage());
            };

            var identity = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    avatar,
                    new Label { Text = name, Style = UI.Style("PageTitle"), HorizontalOptions = LayoutOptions.Center },
                    new Label { Text = Preferences.Get("user_email", string.Empty), TextColor = UI.Color("Muted"), HorizontalOptions = LayoutOptions.Center }
                }
            };

            var privacy = UI.Secondary("Privacy policy", async () =>
                await Launcher.Default.OpenAsync("https://github.com/cn108/plant-app/blob/main/PRIVACY.md"));

            Content = UI.Body(UI.Hero("Your profile", "Account and garden summary.", Icons.Person), UI.Card(identity, 22), UI.Card(stats, 18), privacy, signOut);
        }
    }

    public sealed class UserDetailsPage : ContentPage
    {
        public UserDetailsPage(DBUsers user)
        {
            ArgumentNullException.ThrowIfNull(user);
            Title = "User details";
            var info = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label { Text = user.Username ?? "User", Style = UI.Style("PageTitle") },
                    new Label { Text = user.Email ?? "No email" },
                    UI.Text($"Joined {user.CreatedAt:dd MMM yyyy}", "Caption")
                }
            };
            Content = UI.Body(UI.Hero("User details", "Account information.", Icons.Person), UI.Card(info, 20));
        }
    }
}
