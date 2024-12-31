using SQLite;

namespace FinalYearProject
{
    [Table("DBUser")]
    public class DBUsers
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}