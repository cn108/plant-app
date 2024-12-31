using Microsoft.Maui.Controls;
using System;
using System.Linq;

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
            var email = EmailEntry.Text?.Trim().ToLower();
            var password = PasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Email and Password are required.", "OK");
                return;
            }

            var user = _plantAppDatabase.GetUserByEmail(email);
            if (user == null)
            {
                await DisplayAlert("Error", "Invalid email or password.", "OK");
                return;
            }

            // For debugging purposes
            Console.WriteLine($"Stored Hashed Password: {user.Password}");
            Console.WriteLine($"Password Verification Result: {BCrypt.Net.BCrypt.Verify(password, user.Password)}");

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                await DisplayAlert("Error", "Invalid password.", "OK");
                return;
            }

            // Store user session using Preferences
            Preferences.Set("user_id", user.UserId);
            Preferences.Set("user_email", user.Email);
            Preferences.Set("user_username", user.Username);
            await Navigation.PushAsync(new DashboardPage());
            Application.Current.MainPage = new AppShell();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistrationPage());
        }
    }
}
