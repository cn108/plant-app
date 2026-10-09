using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FinalYearProject
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ImageSource? photo;

        [ObservableProperty]
        private string? outputLabel = "Choose or take a photo to identify a plant.";

        [ObservableProperty]
        private bool isRunning;

        public MainPageViewModel()
        {
            PickPhotoCommand = new AsyncRelayCommand(PickPhotoAsync, () => !IsRunning);
            TakePhotoCommand = new AsyncRelayCommand(TakePhotoAsync, () => !IsRunning);
        }

        public IAsyncRelayCommand PickPhotoCommand { get; }
        public IAsyncRelayCommand TakePhotoCommand { get; }

        partial void OnIsRunningChanged(bool value)
        {
            PickPhotoCommand.NotifyCanExecuteChanged();
            TakePhotoCommand.NotifyCanExecuteChanged();
        }

        private async Task PickPhotoAsync()
        {
            try
            {
                var photoResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Pick a picture"
                });

                if (photoResult is not null)
                {
                    await ProcessPhotoAsync(photoResult);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Picking a plant photo failed: {ex}");
                OutputLabel = "Unable to open the photo picker. Check photo permissions and try again.";
            }
        }

        private async Task TakePhotoAsync()
        {
            try
            {
                var photoResult = await MediaPicker.CapturePhotoAsync();
                if (photoResult is not null)
                {
                    await ProcessPhotoAsync(photoResult);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Taking a plant photo failed: {ex}");
                OutputLabel = "Unable to take a photo. Check camera permissions and try again.";
            }
        }

        private async Task ProcessPhotoAsync(FileResult photoResult)
        {
            IsRunning = true;
            OutputLabel = "Analyzing photo...";

            try
            {
                await using var stream = await photoResult.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var predictions = await Task.Run(() =>
                    MLModel1.PredictAllLabels(new MLModel1.ModelInput
                    {
                        ImageSource = imageBytes
                    })
                    .Take(3)
                    .ToArray());

                if (predictions.Length == 0)
                {
                    OutputLabel = "No plant predictions were returned. Try another photo.";
                    return;
                }

                Photo = ImageSource.FromStream(() => new MemoryStream(imageBytes, writable: false));
                OutputLabel = "Top predictions:" + Environment.NewLine +
                    string.Join(Environment.NewLine, predictions.Select(
                        prediction => $"{prediction.Key}: {prediction.Value:P2}"));
            }
            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Plant identification model is missing: {ex}");
                OutputLabel = "Plant identification is unavailable because the trained model file is missing.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Plant identification failed: {ex}");
                OutputLabel = "Unable to identify this photo. Try another image.";
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
