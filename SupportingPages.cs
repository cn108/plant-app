using System.Net.Mail;
using FinalYearProject.Models;

namespace FinalYearProject
{
    public sealed class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            Title = "Welcome";
            Content = new VerticalStackLayout
            {
                Padding = 28,
                Spacing = 18,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "Plant Companion",
                        FontSize = 32,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#1B5E20"),
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Label
                    {
                        Text = "Identify plants, plan crop rotations, and keep track of plant care.",
                        FontSize = 17,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    CreateButton("Sign in", async () =>
                        await Navigation.PushAsync(new LoginPage())),
                    CreateButton("Create an account", async () =>
                        await Navigation.PushAsync(new RegistrationPage()))
                }
            };
        }

        private static Button CreateButton(string text, Func<Task> action)
        {
            var button = new Button
            {
                Text = text,
                BackgroundColor = Color.FromArgb("#2E7D32"),
                TextColor = Colors.White,
                CornerRadius = 10
            };
            button.Clicked += async (_, _) => await action();
            return button;
        }
    }

    public sealed class RegistrationPage : ContentPage
    {
        private readonly Entry _usernameEntry = new() { Placeholder = "Name", ReturnType = ReturnType.Next };
        private readonly Entry _emailEntry = new() { Placeholder = "Email", Keyboard = Keyboard.Email, ReturnType = ReturnType.Next };
        private readonly Entry _passwordEntry = new() { Placeholder = "Password (at least 8 characters)", IsPassword = true, ReturnType = ReturnType.Done };
        private readonly Label _statusLabel = new() { TextColor = Colors.DarkRed };

        public RegistrationPage()
        {
            Title = "Create account";
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 24,
                    Spacing = 14,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = "Create your account", FontSize = 28, FontAttributes = FontAttributes.Bold },
                        _usernameEntry,
                        _emailEntry,
                        _passwordEntry,
                        _statusLabel,
                        new Button
                        {
                            Text = "Register",
                            BackgroundColor = Color.FromArgb("#2E7D32"),
                            TextColor = Colors.White,
                            Command = new Command(async () => await RegisterAsync())
                        }
                    }
                }
            };
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
                _statusLabel.Text = "Name, email, and password are required.";
                return;
            }

            try
            {
                _ = new MailAddress(email);
            }
            catch (FormatException)
            {
                _statusLabel.Text = "Enter a valid email address.";
                return;
            }

            if (password.Length < 8)
            {
                _statusLabel.Text = "Choose a password with at least 8 characters.";
                return;
            }

            try
            {
                var user = new DBUsers
                {
                    Username = username,
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password)
                };
                App.Database.AddUser(user);
                App.SetCurrentUser(user.UserId, username);
                Preferences.Set("user_id", user.UserId);
                Preferences.Set("user_email", user.Email);
                Preferences.Set("user_username", username);
                Application.Current!.MainPage = new AppShell();
            }
            catch (InvalidOperationException)
            {
                _statusLabel.Text = "An account with this email already exists.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Account registration failed: {ex}");
                _statusLabel.Text = "Unable to create the account right now. Please try again.";
            }
        }
    }

    public sealed class WeatherPage : ContentPage
    {
        private readonly Label _weatherLabel = new() { Text = "Loading current weather...", FontSize = 22 };

        public WeatherPage()
        {
            Title = "Weather";
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 16,
                Children =
                {
                    new Label { Text = "Current weather", FontSize = 28, FontAttributes = FontAttributes.Bold },
                    new Label { Text = "London, United Kingdom", FontSize = 16 },
                    _weatherLabel,
                    new Button { Text = "Refresh", Command = new Command(async () => await LoadWeatherAsync()) }
                }
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadWeatherAsync();
        }

        private async Task LoadWeatherAsync()
        {
            _weatherLabel.Text = "Loading current weather...";
            try
            {
                var weather = await new WeatherService().GetWeatherAsync(51.5074, -0.1278);
                _weatherLabel.Text = $"Temperature: {weather.Temperature:0.#} °C";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loading current weather failed: {ex}");
                _weatherLabel.Text = "Weather is unavailable. Check your connection and try again.";
            }
        }
    }

    public sealed class VegetablesPage : ContentPage
    {
        private static readonly string[] Vegetables =
        {
            "Beans", "Broccoli", "Cabbage", "Carrots", "Cucumber", "Lettuce",
            "Onions", "Peppers", "Spinach", "Tomatoes"
        };

        public VegetablesPage()
        {
            Title = "Vegetables";
            Content = new CollectionView
            {
                ItemsSource = Vegetables,
                ItemTemplate = new DataTemplate(() =>
                {
                    var label = new Label
                    {
                        Padding = new Thickness(16, 12),
                        FontSize = 18
                    };
                    label.SetBinding(Label.TextProperty, ".");
                    return label;
                })
            };
        }
    }

    public sealed class ViewPlantsPage : ContentPage
    {
        private readonly PlantAppDatabase _database = new();
        private readonly CollectionView _plants = new();

        public ViewPlantsPage()
        {
            Title = "My plants";
            _plants.ItemTemplate = new DataTemplate(() =>
            {
                var name = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold };
                name.SetBinding(Label.TextProperty, nameof(DBPlants.PlantName));
                var detail = new Label { TextColor = Colors.Gray };
                detail.SetBinding(Label.TextProperty, nameof(DBPlants.Season));
                return new VerticalStackLayout
                {
                    Padding = 14,
                    Children = { name, detail }
                };
            });
            Content = _plants;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _plants.ItemsSource = _database.GetPlantsByUser(App.CurrentUserId);
        }
    }

    public sealed class PlantCareTipsPage : ContentPage
    {
        public PlantCareTipsPage()
        {
            Title = "Plant care tips";
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = 24,
                    Spacing = 14,
                    Children =
                    {
                        new Label { Text = "Plant care tips", FontSize = 28, FontAttributes = FontAttributes.Bold },
                        Tip("Water at soil level and check moisture before watering again; many plants are harmed by waterlogged soil."),
                        Tip("Match each plant to its light needs. Rotate indoor pots periodically for even growth."),
                        Tip("Use pots with drainage holes and empty standing water from saucers."),
                        Tip("Remove dead leaves and check regularly for pests, especially beneath leaves."),
                        Tip("Feed plants only during their active growing season and follow the product instructions.")
                    }
                }
            };
        }

        private static Label Tip(string text) => new()
        {
            Text = $"• {text}",
            FontSize = 17,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    public sealed class PlantCareTimelinePage : ContentPage
    {
        private readonly PlantAppDatabase _database = new();
        private readonly CollectionView _tasks = new();

        public PlantCareTimelinePage()
        {
            Title = "Plant care timeline";
            _tasks.ItemTemplate = new DataTemplate(() =>
            {
                var plant = new Label { FontAttributes = FontAttributes.Bold };
                plant.SetBinding(Label.TextProperty, nameof(PlantCareTask.PlantName));
                var description = new Label();
                description.SetBinding(Label.TextProperty, nameof(PlantCareTask.TaskDescription));
                var dueDate = new Label { TextColor = Colors.Gray };
                dueDate.SetBinding(Label.TextProperty, nameof(PlantCareTask.DueDate), stringFormat: "Due {0:dd MMM yyyy}");
                return new VerticalStackLayout
                {
                    Padding = 14,
                    Children = { plant, description, dueDate }
                };
            });
            Content = _tasks;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _tasks.ItemsSource = _database.GetPlantCareTasksByUser(App.CurrentUserId);
        }
    }

    public sealed class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            Title = "Profile";
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 14,
                Children =
                {
                    new Label { Text = "Your profile", FontSize = 28, FontAttributes = FontAttributes.Bold },
                    new Label { Text = $"Name: {App.CurrentUserName}", FontSize = 18 },
                    new Label { Text = $"Email: {Preferences.Get("user_email", string.Empty)}", FontSize = 18 },
                    new Button
                    {
                        Text = "Sign out",
                        Command = new Command(() =>
                        {
                            UserService.LogoutUser();
                            Application.Current!.MainPage = new NavigationPage(new WelcomePage());
                        })
                    }
                }
            };
        }
    }

    public sealed class UserDetailsPage : ContentPage
    {
        public UserDetailsPage(DBUsers user)
        {
            ArgumentNullException.ThrowIfNull(user);
            Title = "User details";
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 12,
                Children =
                {
                    new Label { Text = user.Username ?? "User", FontSize = 26, FontAttributes = FontAttributes.Bold },
                    new Label { Text = user.Email ?? "No email", FontSize = 18 },
                    new Label { Text = $"Joined {user.CreatedAt:dd MMM yyyy}", FontSize = 15, TextColor = Colors.Gray }
                }
            };
        }
    }
}
