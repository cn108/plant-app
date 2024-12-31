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
    }
}