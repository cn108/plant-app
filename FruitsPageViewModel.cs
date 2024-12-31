using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace FinalYearProject
{
    public class FruitsPageViewModel : INotifyPropertyChanged
    {
        private readonly PlantAppDatabase _plantRepository;
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

                    if (selectedFruit != null)
                    {
                        // Navigate to FruitDetailPage when a fruit is selected
                        Application.Current.MainPage.Navigation.PushAsync(new FruitDetailPage
                        {
                            BindingContext = selectedFruit
                        });

                        // Load plant care tips related to the selected fruit
                        
                    }
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

        // Constructor for ViewModel
        public FruitsPageViewModel() : this(App.CurrentUserId)
        {
        }

        // Overloaded constructor to pass the current user ID
        public FruitsPageViewModel(int userId)
        {
            _plantRepository = new PlantAppDatabase(); // Assuming PlantAppDatabase provides access to data
            _taskRepository = new PlantAppDatabase();

            FilteredPlants = new ObservableCollection<DBPlants>();
            LoadFruits(); // Load predefined fruits for this user

        }

        // Method to load predefined fruits specific to the user
        private void LoadFruits()
        {
            var predefinedFruits = new List<DBPlants>
            {
                new DBPlants { PId = 1, UserId = App.CurrentUserId, PlantName = "Apple", ImagePath =  "apple.jpg" , Season = "Fall", WaterPerLiters = 2 },
                new DBPlants { PId = 2, UserId = App.CurrentUserId, PlantName = "Banana", ImagePath =  "banana.jpg", Season = "All Year", WaterPerLiters = 3 },
                new DBPlants { PId = 3, UserId = App.CurrentUserId, PlantName = "Orange", ImagePath = "orange.jpg", Season = "Winter", WaterPerLiters = 2 },
                new DBPlants { PId = 4, UserId = App.CurrentUserId, PlantName = "Grapes", ImagePath = "grapes.jpg", Season = "Summer", WaterPerLiters = 1 },
                new DBPlants { PId = 5, UserId = App.CurrentUserId, PlantName = "Strawberry", ImagePath = "strawberry.jpg", Season = "Spring", WaterPerLiters = 1 },
                new DBPlants { PId = 6, UserId = App.CurrentUserId, PlantName = "Mango", ImagePath = "mango.jpg", Season = "Summer", WaterPerLiters = 4 },
                new DBPlants { PId = 7, UserId = App.CurrentUserId, PlantName = "Pineapple", ImagePath = "pineapple.jpg", Season = "Summer", WaterPerLiters = 3 },
                new DBPlants { PId = 8, UserId = App.CurrentUserId, PlantName = "Peach", ImagePath = "peach.jpg", Season = "Summer", WaterPerLiters = 2 },
                new DBPlants { PId = 9, UserId = App.CurrentUserId, PlantName = "Plum", ImagePath = "plum.jpg", Season = "Summer", WaterPerLiters = 2 },
                new DBPlants { PId = 10, UserId = App.CurrentUserId, PlantName = "Watermelon", ImagePath = "watermelon.jpg", Season = "Summer", WaterPerLiters = 5 },
                new DBPlants { PId = 11, UserId = App.CurrentUserId, PlantName = "Lemon", ImagePath = "lemon.jpg", Season = "All Year", WaterPerLiters = 2 },
                new DBPlants { PId = 12, UserId = App.CurrentUserId, PlantName = "Kiwi", ImagePath = "kiwi.jpg", Season = "Winter", WaterPerLiters = 2 },
                new DBPlants { PId = 13, UserId = App.CurrentUserId, PlantName = "Cherry", ImagePath = "cherry.jpg", Season = "Summer", WaterPerLiters = 1 },
                new DBPlants { PId = 14, UserId = App.CurrentUserId, PlantName = "Avocado", ImagePath = "avocado.jpg", Season = "Spring", WaterPerLiters = 3 },
                new DBPlants { PId = 15, UserId = App.CurrentUserId, PlantName = "Papaya", ImagePath = "papaya.jpg", Season = "Summer", WaterPerLiters = 3 }
            };

            foreach (var fruit in predefinedFruits)
            {
                FilteredPlants.Add(fruit);
            }

            OnPropertyChanged(nameof(FilteredPlants));
        }


       

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
