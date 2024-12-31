using System.Collections.ObjectModel;
using FinalYearProject.Models;

namespace FinalYearProject;

public partial class CropRotationPlannerPage : ContentPage
{
    private readonly PlantAppDatabase _database;
    private List<Crop> _availableCrops;

    public CropRotationPlannerPage()
    {
        try
        {
            InitializeComponent();
            _database = new PlantAppDatabase();
            InitializeCrops();
            LoadSavedConfiguration();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CropRotationPlannerPage initialization error: {ex}");
            DisplayAlert("Error", "Failed to initialize the page", "OK");
        }
    }

    private void InitializeCrops()
    {
        _availableCrops = new List<Crop>
        {
            new Crop { Name = "Tomatoes", Season = "Summer" },
            new Crop { Name = "Lettuce", Season = "Spring/Fall" },
            new Crop { Name = "Carrots", Season = "Spring" },
            new Crop { Name = "Peas", Season = "Spring" },
            new Crop { Name = "Beans", Season = "Summer" },
            new Crop { Name = "Cabbage", Season = "Fall" }
        };

        CropsCollection.ItemsSource = _availableCrops;
    }

    private void LoadSavedConfiguration()
    {
        try
        {
            var beds = _database.GetAllBeds();
            foreach (var bed in beds)
            {
                UpdateBedLabel(bed.Position, bed.CurrentCrop);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadSavedConfiguration error: {ex}");
            DisplayAlert("Error", "Failed to load saved configuration", "OK").ConfigureAwait(false);
        }
    }

    private void UpdateBedLabel(int position, string cropName)
    {
        switch (position)
        {
            case 1:
                Bed1Label.Text = string.IsNullOrEmpty(cropName) ? "Bed 1" : cropName;
                break;
            case 2:
                Bed2Label.Text = string.IsNullOrEmpty(cropName) ? "Bed 2" : cropName;
                break;
            case 3:
                Bed3Label.Text = string.IsNullOrEmpty(cropName) ? "Bed 3" : cropName;
                break;
            case 4:
                Bed4Label.Text = string.IsNullOrEmpty(cropName) ? "Bed 4" : cropName;
                break;
        }
    }

    private void OnDrop(object sender, DropEventArgs e)
    {
        try
        {
            if (e.Data.Properties.ContainsKey("Crop"))
            {
                var crop = e.Data.Properties["Crop"] as Crop;
                var frame = sender as Frame;
                var label = frame.Content as Label;
                label.Text = crop.Name;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnDrop error: {ex}");
            DisplayAlert("Error", "Failed to drop crop", "OK").ConfigureAwait(false);
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            SaveBed(1, Bed1Label.Text);
            SaveBed(2, Bed2Label.Text);
            SaveBed(3, Bed3Label.Text);
            SaveBed(4, Bed4Label.Text);

            await DisplayAlert("Success", "Crop rotation plan saved!", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnSaveClicked error: {ex}");
            await DisplayAlert("Error", "Failed to save configuration", "OK");
        }
    }

    private void SaveBed(int position, string cropName)
    {
        var bed = new CropBed
        {
            Position = position,
            CurrentCrop = cropName == $"Bed {position}" ? null : cropName
        };
        _database.SaveBedConfiguration(bed);
    }
}