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
            AppDomain.CurrentDomain.UnhandledException += (_, e) => LogCrash(e.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (_, e) => LogCrash(e.Exception);
#if WINDOWS
            Microsoft.UI.Xaml.Application.Current.UnhandledException += (_, e) =>
            {
                LogCrash(e.Exception);
                e.Handled = true;
            };
#endif
            UserAppTheme = AppTheme.Light;
            Database = new PlantAppDatabase();
            _ = ApiClient.RestoreAsync();
            NavigateToAppropriatePage();
        }

        private static void LogCrash(object? error)
        {
            try
            {
                File.AppendAllText(Path.Combine(Path.GetTempPath(), "plantapp-crash.log"), $"{DateTime.Now:O}{Environment.NewLine}{error}{Environment.NewLine}");
            }
            catch { /* logging must never throw */ }
        }

        public static void SetCurrentUser(int userId, string userName)
        {
            CurrentUserId = userId;
            CurrentUserName = userName;
        }

        public void NavigateToAppropriatePage()
        {
            var userId = Preferences.Get("user_id", 0);
            var email = Preferences.Get("user_email", string.Empty);
            var username = Preferences.Get("user_username", string.Empty);

            if (userId > 0 && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(username))
            {
                SetCurrentUser(userId, username);
                MainPage = new AppShell();
            }
            else
            {
                MainPage = new NavigationPage(new WelcomePage());
            }
        }
    }
}
