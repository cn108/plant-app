using System.Net.Http;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace FinalYearProject
{
    public class DBPlantsListViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        // Observable collection to hold the filtered plants
        public ObservableCollection<DBPlants> FilteredPlants { get; set; }

        // Constructor for the view model
        public DBPlantsListViewModel()
        {
            _httpClient = new HttpClient();
            FilteredPlants = new ObservableCollection<DBPlants>();
            LoadAllPlants();
        }

        // The Plants property that holds the details of all plants
        private ObservableCollection<DBPlants> allPlants;
        public ObservableCollection<DBPlants> AllPlants
        {
            get { return allPlants; }
            set
            {
                if (value != null)
                {
                    allPlants = value;
                    OnPropertyChanged(nameof(AllPlants));
                }
            }
        }

        // Method to load all plants asynchronously from the API
        private async void LoadAllPlants()
        {
            try
            {
                // Assuming your API is running at http://localhost:5000/api/plants
                var response = await _httpClient.GetAsync("http://localhost:5000/api/plants");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var plants = JsonSerializer.Deserialize<List<DBPlants>>(json);

                    AllPlants = new ObservableCollection<DBPlants>(plants);
                    FilterPlants(SearchTerm); // Apply initial filter if there is a search term
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., API not reachable)
                Console.WriteLine($"Error fetching plants: {ex.Message}");
            }
        }

        // The search term for filtering the plants
        private string searchTerm;
        public string SearchTerm
        {
            get
            {
                return searchTerm;
            }
            set
            {
                if (value != null)
                {
                    searchTerm = value;
                    OnPropertyChanged(nameof(SearchTerm));
                    FilterPlants(searchTerm);
                }
            }
        }

        // Method to filter plants based on the search term
        public void FilterPlants(string searchText)
        {
            if (AllPlants == null) return; // Handle case when plants haven't loaded yet

            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredPlants = new ObservableCollection<DBPlants>(AllPlants);
            }
            else
            {
                FilteredPlants = new ObservableCollection<DBPlants>(
                    AllPlants.Where(plant => plant.PlantName.ToLower().Contains(searchText.ToLower())));
            }
            OnPropertyChanged(nameof(FilteredPlants));
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}