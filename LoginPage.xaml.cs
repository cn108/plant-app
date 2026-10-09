using Microsoft.Maui.Controls;
using System;

namespace FinalYearProject
{
    public partial class LoginPage : ContentPage
    {
        private readonly PlantAppDatabase _plantAppDatabase;
        public LoginPage()
        {
            InitializeComponent();
            _plantAppDatabase = new PlantAppDatabase();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim().ToLowerInvariant();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Email and Password are required.", "OK");
                return;
            }

            var user = _plantAppDatabase.GetUserByEmail(email);
            if (user is null ||
                string.IsNullOrEmpty(user.Password) ||
                !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                await DisplayAlert("Error", "Invalid email or password.", "OK");
                return;
            }

            Preferences.Set("user_id", user.UserId);
            Preferences.Set("user_email", user.Email);
            Preferences.Set("user_username", user.Username ?? string.Empty);
            App.SetCurrentUser(user.UserId, user.Username ?? string.Empty);
            Application.Current.MainPage = new AppShell();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistrationPage());
        }
    }
}
