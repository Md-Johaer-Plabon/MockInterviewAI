using SQLite;

namespace MockInterviewAI.Model
{
    public class UserData
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; } = 1;
        public string About { get; set; }
        public string Skills { get; set; }
        public string Projects { get; set; }
        public string Technologies { get; set; }
        public string Experience { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
