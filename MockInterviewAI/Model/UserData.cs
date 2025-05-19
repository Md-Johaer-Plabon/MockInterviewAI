using SQLite;

namespace MockInterviewAI.Model
{
    public class UserData
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; } = 1;
        public string Professional_summary { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public string Certifications { get; set; } = string.Empty;
        public string Projects { get; set; } = string.Empty;
        public string Technologies { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string Extracurricular { get; set; } = string.Empty;
        public string Interests { get; set; } = string.Empty;
        public string AdditionalInfo { get; set; } = string.Empty;
    }
}
