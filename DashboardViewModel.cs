using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FinalYearProject.Models;
using SQLite;

namespace FinalYearProject
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private PlantAppDatabase _database;
        private WeatherService _weatherService;
        public static int CurrentUserId { get; private set; }
        public ObservableCollection<DBPlants> FilteredPlants { get; set; }
        private List<DBPlants> predefinedPlants;
        public ObservableCollection<DBPlants> UserPlants { get; set; }
        public ObservableCollection<PlantCareTask> CareTasks { get; set; }
        public ObservableCollection<PlantCareTask> PlantCareTimeline { get; set; }
        public ObservableCollection<PlantCareTask> Reminders { get; }
        public ICommand MarkAsDoneCommand { get; }
        public ICommand AddPlantCommand { get; }
        public ICommand NavigateToWeatherCommand { get; }
        public ICommand NavigateToFruitsCommand { get; }
        public ICommand NavigateToVegetablesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NavigateToChatCommand { get; }
        public ICommand NavigateToViewPlantsCommand { get; }
        public ICommand NavigateToCreateForumCommand { get; }
        public ICommand NavigateToCropRotationCommand { get; }
        public ICommand NavigateToPlantCareTipsCommand { get; }
        public ICommand NavigatedToProfileCommand { get; }
        public ICommand LogoutCommand { get; }

        private double _temperature;
        public double Temperature
        {
            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged(nameof(Temperature));
            }
        }

        private bool _isFetchingWeather;
        public bool IsFetchingWeather
        {
            get => _isFetchingWeather;
            set
            {
                _isFetchingWeather = value;
                OnPropertyChanged(nameof(IsFetchingWeather));
            }
        }

        private string _weatherMessage = "Loading current weather...";
        public string WeatherMessage
        {
            get => _weatherMessage;
            set
            {
                _weatherMessage = value;
                OnPropertyChanged(nameof(WeatherMessage));
            }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged(nameof(SearchQuery));
                    // Filter the plants when the query changes
                    //FilterPlants();
                }
            }
        }


        public DashboardViewModel()
        {
            _database = new PlantAppDatabase();
            _weatherService = new WeatherService();
            LoadCareTasks();
            LoadPlantCareTimeline();
            UserPlants = _database.GetPlantsByUser(App.CurrentUserId);
            FilteredPlants = new ObservableCollection<DBPlants>(UserPlants);

            //predefinedPlants = _database.GetAllPlants(); // Example repository method

            //FilteredPlants = new ObservableCollection<DBPlants>(predefinedPlants);

            AddPlantCommand = new Command(AddPlant);

            NavigateToWeatherCommand = new Command(NavigateToWeather);
            NavigateToFruitsCommand = new Command(NavigateToFruits);
            NavigateToVegetablesCommand = new Command(NavigateToVegetables);
            NavigateToChatCommand = new Command(NavigateToChat);
            NavigateToCreateForumCommand = new Command(NavigateToCreateForum);
            NavigateToCropRotationCommand = new Command(async () => await NavigateToCropRotation());
            NavigateToPlantCareTipsCommand = new Command(NavigateToPlantCareTips);
            NavigatedToProfileCommand = new Command(NavigateToProfile);
            LogoutCommand = new Command(Logout);

            SearchCommand = new Command(SearchPlants);
            
            NavigateToViewPlantsCommand = new Command(NavigateToViewPlants);
            
            Reminders = new ObservableCollection<PlantCareTask>();
            MarkAsDoneCommand = new Command<PlantCareTask>(OnMarkAsDone);
            LoadReminders();

            _ = FetchWeatherData();
        }

        //private void FilterPlants()
        //{
        //    if (string.IsNullOrWhiteSpace(SearchQuery))
        //    {
        //        // If no search query, display all plants
        //        FilteredPlants.Clear();
        //        foreach (var plant in predefinedPlants)
        //        {
        //            FilteredPlants.Add(plant);
        //        }
        //    }
        //    else
        //    {
        //        var filteredResults = predefinedPlants
        //            .Where(p => p.PlantName.ToLower().Contains(SearchQuery.ToLower()))
        //            .ToList();

        //        FilteredPlants.Clear();
        //        foreach (var plant in filteredResults)
        //        {
        //            FilteredPlants.Add(plant);
        //        }
        //    }
        //}


        private void LoadReminders()
        {
            var tasks = _database.GetPlantCareTasksByUser(App.CurrentUserId).Where(t => t.DueDate >= DateTime.Now).ToList();
            foreach (var task in tasks)
            {
                Reminders.Add(task);
            }
        }

        private void OnMarkAsDone(PlantCareTask task)
        {
            if (task is null)
            {
                return;
            }

            _database.DeletePlantCareTask(task.TaskId);
            Reminders.Remove(task);
            CareTasks.Remove(task);
            PlantCareTimeline.Remove(task);
        }

        private void LoadCareTasks()
        {
            // Retrieve tasks from the database based on the current user
            var tasks = _database.GetPlantCareTasksByUser(App.CurrentUserId);
            CareTasks = new ObservableCollection<PlantCareTask>(tasks);
        }

        private void LoadPlantCareTimeline()
        {
            // Fetch care tasks from the database for the current user
            var tasks = _database.GetPlantCareTasksByUser(App.CurrentUserId);
            PlantCareTimeline = new ObservableCollection<PlantCareTask>(tasks);
        }

        private void AddPlant() { Application.Current.MainPage.Navigation.PushAsync(new AddPlantPage()); }

        private async Task FetchWeatherData()
        {
            IsFetchingWeather = true;
            try
            {
                var weatherData = await _weatherService.GetWeatherAsync(51.5074, -0.1278);
                Temperature = weatherData.Temperature;
                WeatherMessage = $"Current temperature: {Temperature:0.#} °C";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loading dashboard weather failed: {ex}");
                WeatherMessage = "Weather is unavailable. Check your connection.";
            }
            finally
            {
                IsFetchingWeather = false;
            }
        }

        private async void Logout()
        {
            UserService.LogoutUser();
            Application.Current!.MainPage = new NavigationPage(new WelcomePage());
        }

        private async void NavigateToWeather() { await Application.Current.MainPage.Navigation.PushAsync(new WeatherPage()); }

        private async void NavigateToFruits() { await Application.Current.MainPage.Navigation.PushAsync(new FruitsPage());}

        private async void NavigateToVegetables() { await Application.Current.MainPage.Navigation.PushAsync(new VegetablesPage()); }

        private async void NavigateToChat() { await Application.Current.MainPage.Navigation.PushAsync(new ChatPage()); }

        private async Task NavigateToCropRotation()
        {
            try
            {
                var cropRotationPage = new CropRotationPlannerPage();
                await Application.Current.MainPage.Navigation.PushAsync(cropRotationPage);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    $"Navigation failed: {ex.Message}", "OK");
                // For debugging
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
            }
        }
        private async void NavigateToCreateForum() { await Application.Current.MainPage.Navigation.PushAsync(new ForumPage()); }
        
        private DBPlants selectedPlant = new DBPlants();
        
        private async void NavigateToViewPlants() { await Application.Current.MainPage.Navigation.PushAsync(new ViewPlantsPage()); }

        private async void NavigateToPlantCareTips() { await Application.Current.MainPage.Navigation.PushAsync(new PlantCareTipsPage()); }

        private async void NavigateToProfile() { await Application.Current.MainPage.Navigation.PushAsync(new ProfilePage()); }

        private void SearchPlants()
        {
            FilteredPlants = _database.SearchPlants(SearchQuery);
            OnPropertyChanged(nameof(FilteredPlants));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}