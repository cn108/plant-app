using SQLite;

namespace FinalYearProject
{
    [Table("DBPlant")]
    public class DBPlants
    {
        [PrimaryKey, AutoIncrement]
        public int PId { get; set; }

        [Indexed]
        public int UserId { get; set; }
        public string? PlantName { get; set; }
        public string? ImagePath { get; set; }
        public string? Season { get; set; }
        public int WaterPerLiters { get; set; } 
        public string? PlantImage { get; set; }
        public int WateringIntervalDays { get; set; } = 7;

        [Ignore]
        public string Summary => $"{WaterPerLiters} L every {(WateringIntervalDays <= 0 ? 7 : WateringIntervalDays)} days · {Season}";

        [Ignore]
        public bool HasImage => !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath);
    }
}