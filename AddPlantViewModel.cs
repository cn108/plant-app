using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace FinalYearProject
{
    public class AddPlantViewModel : INotifyPropertyChanged
    {
        private PlantAppDatabase _database;
        public string PlantName { get; set; }
        public string Season { get; set; }
        public ImageSource PlantImage { get; set; }

        private string _waterPerLiters;
        public string WaterPerLiters
        {
            get => _waterPerLiters;
            set
            {
                _waterPerLiters = value;
                OnPropertyChanged(nameof(WaterPerLiters));
            }
        }

        // Define the PlantImagePath property
        private string _plantImagePath;
        public string PlantImagePath
        {
            get => _plantImagePath;
            set
            {
                _plantImagePath = value;
                OnPropertyChanged(nameof(PlantImagePath));
            }
        }

        public ICommand SavePlantCommand { get; }
        public ICommand UploadImageCommand { get; }
        public ICommand CaptureImageCommand { get; }

        public AddPlantViewModel()
        {
            _database = new PlantAppDatabase();
            SavePlantCommand = new Command(SavePlant);
            UploadImageCommand = new Command(UploadImage);
            CaptureImageCommand = new Command(CaptureImage);
        }

        private async void CaptureImage()
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                var stream = await photo.OpenReadAsync();
                PlantImage = ImageSource.FromStream(() => stream);
                PlantImagePath = photo.FullPath;  // Store the full path of the captured image
                OnPropertyChanged(nameof(PlantImage));
            }
        }

        private async void SavePlant()
        {
            if (int.TryParse(WaterPerLiters, out int waterPerLiters))
            {
                var newPlant = new DBPlants
                {
                    PlantName = PlantName,
                    Season = Season,
                    WaterPerLiters = waterPerLiters,
                    ImagePath = PlantImagePath, // Store the image path or convert to base64 string
                    UserId = App.CurrentUserId // Ensure this is the logged-in user's ID
                };

                _database.AddPlant(newPlant);
                // Navigate back to dashboard
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            else
            {
                // Handle invalid input
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid number for Water (Liters).", "OK");
            }
        }

        private async void UploadImage()
        {
            var result = await MediaPicker.PickPhotoAsync();
            if (result != null)
            {
                // Get the full path of the image
                var stream = await result.OpenReadAsync();
                var imagePath = result.FullPath; // Get the full image path

                // Display the image in the app
                PlantImage = ImageSource.FromStream(() => stream);
                OnPropertyChanged(nameof(PlantImage));

                // Save the full image path to the database
                PlantImagePath = imagePath;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
