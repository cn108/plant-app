using CommunityToolkit.Mvvm.Input; // Needed for AsyncRelayCommand
using CommunityToolkit.Mvvm.ComponentModel;

namespace FinalYearProject
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string photo; // This will bind to the Image control in the XAML

        [ObservableProperty]
        private string outputLabel; // This will display the result from the model

        [ObservableProperty]
        private bool isRunning; // Activity indicator binding

        public MainPageViewModel()
        {
            PickPhotoCommand = new AsyncRelayCommand(PickPhotoAsync);
            TakePhotoCommand = new AsyncRelayCommand(TakePhotoAsync);
        }

        public IAsyncRelayCommand PickPhotoCommand { get; }
        public IAsyncRelayCommand TakePhotoCommand { get; }

        private async Task PickPhotoAsync()
        {
            try
            {
                FileResult photoResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Pick a picture"
                });

                if (photoResult != null)
                {
                    await ProcessPhotoAsync(photoResult);
                }
            }
            catch (Exception ex)
            {
                OutputLabel = $"Error: {ex.Message}";
            }
        }

        private async Task TakePhotoAsync()
        {
            try
            {
                FileResult photoResult = await MediaPicker.CapturePhotoAsync();
                if (photoResult != null)
                {
                    await ProcessPhotoAsync(photoResult);
                }
            }
            catch (Exception ex)
            {
                OutputLabel = $"Error: {ex.Message}";
            }
        }

        private async Task ProcessPhotoAsync(FileResult photoResult)
        {
            IsRunning = true; // Start activity indicator

            // Convert image to byte array for ML model
            var stream = await photoResult.OpenReadAsync();
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                byte[] imageBytes = ms.ToArray();

                // Create single instance of sample data for model input
                var sampleData = new MLModel1.ModelInput
                {
                    ImageSource = imageBytes
                };

                // Make a single prediction
                var sortedScoresWithLabel = MLModel1.PredictAllLabels(sampleData);

                // Sort the predictions and take the top 3
                var top3Scores = sortedScoresWithLabel
                    .OrderByDescending(kv => kv.Value)
                    .Take(3);

                // Create a formatted string to display the top 3 predictions
                string output = "Top 3 Predictions:\n";
                foreach (var score in top3Scores)
                {
                    output += $"{score.Key}: {score.Value:P2}\n";  // Format score as percentage
                }

                // Set the OutputLabel to display the top 3 predictions
                OutputLabel = output;

                // Bind the image to the UI
                Photo = photoResult.FullPath;
            }

            IsRunning = false; // Stop activity indicator
        }

    }

}
