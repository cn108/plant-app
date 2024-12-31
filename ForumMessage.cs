using SQLite;

namespace FinalYearProject
{
    [Table("ForumMessages")]
    public class ForumMessage
    {
        [PrimaryKey, AutoIncrement]
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public string Category { get; set; }
        public string MessageText { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
    }
}
