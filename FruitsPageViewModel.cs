using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FinalYearProject.Models;

namespace FinalYearProject
{
    public class FruitsPageViewModel : INotifyPropertyChanged
    {
        private readonly PlantAppDatabase _taskRepository;

        // Collection to hold the predefined fruits
        public ObservableCollection<DBPlants> FilteredPlants { get; set; }

        private DBPlants selectedFruit;
        public DBPlants SelectedFruit
        {
            get => selectedFruit;
            set
            {
                if (selectedFruit != value)
                {
                    selectedFruit = value;
                    OnPropertyChanged(nameof(SelectedFruit));
                }
            }
        }

        private ImageSource _imagepath;
        public ImageSource ImagePath
        {
            get { return _imagepath; }
            set
            {
                _imagepath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        private string _plantName;
        public string PlantName
        {
            get { return _plantName; }
            set
            {
                _plantName = value;
                OnPropertyChanged(nameof(PlantName));
            }
        }

        private string _plantImage;
        public string PlantImage
        {
            get => _plantImage;
            set
            {
                _plantImage = value;
                OnPropertyChanged(nameof(PlantImage));
            }
        }

        // Command to add the selected fruit to the care timeline
        public ICommand AddToTimelineCommand { get; private set; }
        public ICommand ViewDetailsCommand { get; private set; }
        private readonly int _userId;

        // Constructor for ViewModel
        public FruitsPageViewModel() : this(App.CurrentUserId)
        {
        }

        // Overloaded constructor to pass the current user ID
        public FruitsPageViewModel(int userId)
        {
            _userId = userId;
            _taskRepository = new PlantAppDatabase();

            FilteredPlants = new ObservableCollection<DBPlants>();
            AddToTimelineCommand = new Command(async () => await AddSelectedFruitToTimelineAsync());
            ViewDetailsCommand = new Command(async () => await ViewSelectedFruitAsync());
            LoadFruits(); // Load predefined fruits for this user

        }

        // Method to load predefined fruits specific to the user
        private void LoadFruits()
        {
            var predefinedFruits = new List<DBPlants>
            {
                new DBPlants { PId = 1, UserId = _userId, PlantName = "Apple", Season = "Fall", WaterPerLiters = 2 },
                new DBPlants { PId = 2, UserId = _userId, PlantName = "Banana", Season = "All Year", WaterPerLiters = 3 },
                new DBPlants { PId = 3, UserId = _userId, PlantName = "Orange", Season = "Winter", WaterPerLiters = 2 },
                new DBPlants { PId = 4, UserId = _userId, PlantName = "Grapes", Season = "Summer", WaterPerLiters = 1 },
                new DBPlants { PId = 5, UserId = _userId, PlantName = "Strawberry", Season = "Spring", WaterPerLiters = 1 },
                new DBPlants { PId = 6, UserId = _userId, PlantName = "Mango", Season = "Summer", WaterPerLiters = 4 },
                new DBPlants { PId = 7, UserId = _userId, PlantName = "Pineapple", Season = "Summer", WaterPerLiters = 3 },
                new DBPlants { PId = 8, UserId = _userId, PlantName = "Peach", Season = "Summer", WaterPerLiters = 2 },
                new DBPlants { PId = 9, UserId = _userId, PlantName = "Plum", Season = "Summer", WaterPerLiters = 2 },
                new DBPlants { PId = 10, UserId = _userId, PlantName = "Watermelon", Season = "Summer", WaterPerLiters = 5 },
                new DBPlants { PId = 11, UserId = _userId, PlantName = "Lemon", Season = "All Year", WaterPerLiters = 2 },
                new DBPlants { PId = 12, UserId = _userId, PlantName = "Kiwi", Season = "Winter", WaterPerLiters = 2 },
                new DBPlants { PId = 13, UserId = _userId, PlantName = "Cherry", Season = "Summer", WaterPerLiters = 1 },
                new DBPlants { PId = 14, UserId = _userId, PlantName = "Avocado", Season = "Spring", WaterPerLiters = 3 },
                new DBPlants { PId = 15, UserId = _userId, PlantName = "Papaya", Season = "Summer", WaterPerLiters = 3 }
            };

            foreach (var fruit in predefinedFruits)
            {
                FilteredPlants.Add(fruit);
            }

            OnPropertyChanged(nameof(FilteredPlants));
        }

        private async Task AddSelectedFruitToTimelineAsync()
        {
            if (SelectedFruit is null)
            {
                return;
            }

            _taskRepository.AddPlantCareTask(new PlantCareTask
            {
                UserId = _userId,
                PlantName = SelectedFruit.PlantName ?? "Plant",
                TaskDescription = $"Water the plant ({SelectedFruit.WaterPerLiters} L).",
                TaskDate = DateTime.Now,
                DueDate = DateTime.Today.AddDays(1)
            });

            await Application.Current!.MainPage!.DisplayAlert(
                "Added",
                $"{SelectedFruit.PlantName} was added to your care timeline.",
                "OK");
        }

        private async Task ViewSelectedFruitAsync()
        {
            if (SelectedFruit is not null)
            {
                await Application.Current!.MainPage!.Navigation.PushAsync(new FruitDetailPage
                {
                    BindingContext = SelectedFruit
                });
            }
        }


       

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
