namespace FinalYearProject
{
    public static class UserService
    {
        public static void LogoutUser()
        {
            Preferences.Remove("user_id");
            Preferences.Remove("user_email");
            Preferences.Remove("user_username");
            ApiClient.SignOut();
            App.SetCurrentUser(0, string.Empty);
        }
    }
}
