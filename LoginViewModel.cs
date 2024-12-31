using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace FinalYearProject
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private PlantAppDatabase _database;
        private string _email;
        private string _password;

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _database = new PlantAppDatabase();
            LoginCommand = new Command(OnLogin);
        }

        private void OnLogin()
        {
            var user = _database.GetUserByEmail(Email);

            if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                App.SetCurrentUser(user.UserId, user.Username);
                Application.Current.MainPage = new NavigationPage(new DashboardPage());
            }
            else
            {
                // Handle login failure (e.g., show an error message)
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
