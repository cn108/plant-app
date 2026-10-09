using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Media;

namespace FinalYearProject
{
    public class AddPlantViewModel : INotifyPropertyChanged
    {
        private readonly PlantAppDatabase _database = new();
        private string _plantName = string.Empty;
        private string _season = string.Empty;
        private ImageSource? _plantImage;
        private string _waterPerLiters = string.Empty;
        private string? _plantImagePath;

        public string PlantName
        {
            get => _plantName;
            set
            {
                _plantName = value;
                OnPropertyChanged(nameof(PlantName));
            }
        }

        public string Season
        {
            get => _season;
            set
            {
                _season = value;
                OnPropertyChanged(nameof(Season));
            }
        }

        public ImageSource? PlantImage
        {
            get => _plantImage;
            private set
            {
                _plantImage = value;
                OnPropertyChanged(nameof(PlantImage));
            }
        }

        public string WaterPerLiters
        {
            get => _waterPerLiters;
            set
            {
                _waterPerLiters = value;
                OnPropertyChanged(nameof(WaterPerLiters));
            }
        }

        public string? PlantImagePath
        {
            get => _plantImagePath;
            private set
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
            SavePlantCommand = new Command(async () => await SavePlantAsync());
            UploadImageCommand = new Command(async () => await SelectImageAsync(capture: false));
            CaptureImageCommand = new Command(async () => await SelectImageAsync(capture: true));
        }

        private async Task SelectImageAsync(bool capture)
        {
            try
            {
                var result = capture
                    ? await MediaPicker.CapturePhotoAsync()
                    : await MediaPicker.PickPhotoAsync();
                if (result is null)
                {
                    return;
                }

                await using var source = await result.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await source.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var imagePath = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid():N}.image");
                await File.WriteAllBytesAsync(imagePath, imageBytes);

                PlantImagePath = imagePath;
                PlantImage = ImageSource.FromStream(() => new MemoryStream(imageBytes, writable: false));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Selecting a plant photo failed: {ex}");
                await Application.Current!.MainPage!.DisplayAlert(
                    "Photo unavailable",
                    "Unable to select or save that photo. Check app permissions and try again.",
                    "OK");
            }
        }

        private async Task SavePlantAsync()
        {
            if (string.IsNullOrWhiteSpace(PlantName) ||
                string.IsNullOrWhiteSpace(Season) ||
                !int.TryParse(WaterPerLiters, out var waterPerLiters) ||
                waterPerLiters <= 0)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Check plant details",
                    "Enter a plant name, a season, and a positive water amount in litres.",
                    "OK");
                return;
            }

            try
            {
                _database.AddPlant(new DBPlants
                {
                    PlantName = PlantName.Trim(),
                    Season = Season.Trim(),
                    WaterPerLiters = waterPerLiters,
                    ImagePath = PlantImagePath,
                    UserId = App.CurrentUserId
                });

                await Application.Current!.MainPage!.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Saving a plant failed: {ex}");
                await Application.Current!.MainPage!.DisplayAlert(
                    "Unable to save",
                    "The plant could not be saved. Please try again.",
                    "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
