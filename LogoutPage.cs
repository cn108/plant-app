using Microsoft.Maui.Controls;

namespace FinalYearProject
{
    public class LogoutPage : ContentPage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Preferences.Remove("user_id");
            Preferences.Remove("user_email");
            Preferences.Remove("user_username");
            App.SetCurrentUser(0, string.Empty);
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
