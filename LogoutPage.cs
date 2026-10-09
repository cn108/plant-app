using Microsoft.Maui.Controls;

namespace FinalYearProject
{
    public class LogoutPage : ContentPage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();

            UserService.LogoutUser();
            Application.Current!.MainPage = new NavigationPage(new WelcomePage());
        }
    }
}
