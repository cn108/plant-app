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
                Photo = ImageSource.FromStream(() => new MemoryStream(imageBytes, writable: false));

                var lines = await IdentifyAsync(imageBytes, photoResult.FileName);
                if (lines is null)
                {
                    return;
                }

                OutputLabel = lines.Count == 0
                    ? "No plant was recognised. Try a clear, close-up photo of a leaf or flower."
                    : "Top matches:" + Environment.NewLine + string.Join(Environment.NewLine, lines);
            }
            catch (PlantIdentificationException ex)
            {
                OutputLabel = ex.Message;
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

        private static async Task<List<string>?> IdentifyAsync(byte[] imageBytes, string fileName)
        {
            if (File.Exists(Path.Combine(AppContext.BaseDirectory, "MLModel1.mlnet")))
            {
                var local = await Task.Run(() =>
                    MLModel1.PredictAllLabels(new MLModel1.ModelInput { ImageSource = imageBytes })
                        .Take(3)
                        .Select(p => $"{p.Key}: {p.Value:P0}")
                        .ToList());
                return local;
            }

            var server = await ApiClient.IdentifyAsync(imageBytes, fileName, "image/jpeg");
            if (server.Ok && server.Value is not null)
            {
                return server.Value
                    .Select(m => string.IsNullOrWhiteSpace(m.CommonName)
                        ? $"{m.ScientificName}: {m.Score:P0}"
                        : $"{m.CommonName} ({m.ScientificName}): {m.Score:P0}")
                    .ToList();
            }

            var apiKey = await PlantIdentificationService.GetApiKeyAsync();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                var entered = page is null
                    ? null
                    : await page.DisplayPromptAsync(
                        "Plant identification",
                        "Enter your free Pl@ntNet API key (my.plantnet.org). It is stored securely on this device.",
                        "Save", "Cancel", "API key");

                if (string.IsNullOrWhiteSpace(entered))
                {
                    throw new PlantIdentificationException("Plant identification needs a Pl@ntNet API key. Tap identify again to add one.");
                }

                apiKey = entered.Trim();
                await PlantIdentificationService.SaveApiKeyAsync(apiKey);
            }

            var matches = await PlantIdentificationService.IdentifyAsync(imageBytes, fileName, apiKey);
            return matches
                .Select(m => string.IsNullOrWhiteSpace(m.CommonName)
                    ? $"{m.ScientificName}: {m.Score:P0}"
                    : $"{m.CommonName} ({m.ScientificName}): {m.Score:P0}")
                .ToList();
        }
    }
}
