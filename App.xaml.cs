using Microsoft.Maui.Controls;

namespace FinalYearProject
{
    public partial class App : Application
    {
        public static PlantAppDatabase Database { get; private set; }
        public static int CurrentUserId { get; private set; }
        public static string CurrentUserName { get; private set; }

        public App()
        {
            InitializeComponent();
            // Initialize the database
            Database = new PlantAppDatabase();
            // Set the MainPage to the WelcomePage initially
            MainPage = new NavigationPage(new WelcomePage());
            //MainPage = new AppShell();

        }

        public static void SetCurrentUser(int userId, string userName)
        {
            CurrentUserId = userId;
            CurrentUserName = userName;
        }

        public void NavigateToAppropriatePage()
        {
            if (Preferences.ContainsKey("user_id") && Preferences.ContainsKey("user_email") && Preferences.ContainsKey("user_username"))
            {
                var userId = Preferences.Get("user_id", 0);
                var email = Preferences.Get("user_email", string.Empty);
                var username = Preferences.Get("user_username", string.Empty);

                SetCurrentUser(userId, username);

                MainPage = new NavigationPage(new DashboardPage());
            }
            else
            {
                MainPage = new NavigationPage(new LoginPage());
            }
        }
    }
}
