using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinalYearProject.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FinalYearProject
{
    public partial class CropRotationPlannerViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _currentDate = DateTime.Now;

        public ObservableCollection<PlantCareTask> RotationSchedules { get; set; } = new ObservableCollection<PlantCareTask>();

        public CropRotationPlannerViewModel()
        {
            LoadRotationSchedules();
            RotationSchedules = new ObservableCollection<PlantCareTask>
            {
                new PlantCareTask { BedId = 1, PlantName = "Tomatoes", SuggestedNextCrop = "Legumes" },
                new PlantCareTask { BedId = 2, PlantName = "Carrots", SuggestedNextCrop = "Leafy Greens" }
            };
        }

        private void LoadRotationSchedules()
        {
            RotationSchedules.Clear();

            RotationSchedules.Add(new PlantCareTask
            {
                BedId = 1,
                PlantName = "Tomato",
                PlantingDate = DateTime.Now.AddMonths(-3),
                HarvestDate = DateTime.Now,
                SuggestedNextCrop = "Legumes"
            });

            RotationSchedules.Add(new PlantCareTask
            {
                BedId = 2,
                PlantName = "Carrot",
                PlantingDate = DateTime.Now.AddMonths(-2),
                HarvestDate = DateTime.Now.AddMonths(1),
                SuggestedNextCrop = "Leafy Greens"
            });
        }

        [RelayCommand]
        public void UpdateRotationSchedule()
        {
            foreach (var task in RotationSchedules)
            {
                task.SuggestedNextCrop = GetNextCrop(task.PlantName);
            }

            OnPropertyChanged(nameof(RotationSchedules));
        }

        private string GetNextCrop(string currentPlant)
        {
            if (currentPlant == "Tomatoes")
                return "Legumes";
            if (currentPlant == "Carrots")
                return "Leafy Greens";

            return "Root Vegetables";
        }
    }
}
