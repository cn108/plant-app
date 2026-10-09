using SQLite;

namespace FinalYearProject.Models
{
    [Table("PlantCareTasks")]
    public class PlantCareTask
    {
        [PrimaryKey, AutoIncrement]
        public int TaskId { get; set; }

        [Indexed]
        public int UserId { get; set; }

        [Indexed]
        public int PlantId { get; set; }

        public string PlantName { get; set; } = string.Empty;
        public int BedId { get; set; }
        public string TaskDescription { get; set; } = string.Empty;
        public DateTime TaskDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PlantingDate { get; set; }
        public DateTime HarvestDate { get; set; }
        public string SuggestedNextCrop { get; set; } = string.Empty;
    }
}
