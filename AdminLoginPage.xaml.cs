using Microsoft.Maui.Controls;

namespace FinalYearProject
{
    public partial class AdminLoginPage : ContentPage
    {
        public AdminLoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            // Add your authentication logic here
            bool isAuthenticated = AuthenticateAdmin(username, password);

            if (isAuthenticated)
            {
                await Navigation.PushAsync(new AdminPage());
            }
            else
            {
                await DisplayAlert("Error", "Invalid username or password", "OK");
            }
        }

        private bool AuthenticateAdmin(string username, string password)
        {
            // Replace this with your actual authentication logic
            return username == "admin" && password == "password";
        }
    }
}
