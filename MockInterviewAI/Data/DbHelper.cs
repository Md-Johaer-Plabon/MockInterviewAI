using MockInterviewAI.Model;
using SQLite;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace MockInterviewAI.Data
{
    public class DbHelper
    {
        private static SQLiteAsyncConnection _database;

        public static async Task InitializeDatabase()
        {
            if (_database != null)
                return;

            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "InterviewBot.db");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<UserData>();
        }

        public static async Task SaveUserData(UserData userData)
        {
            await _database.InsertOrReplaceAsync(userData);
        }

        public static async Task<UserData> GetUserData()
        {
            return await _database.Table<UserData>().Where(x => x.UserId == 1).FirstOrDefaultAsync();
        }

        public static async Task DeleteEntity()
        {
            await _database.Table<UserData>().Where(x => x.UserId == 1).DeleteAsync();
        }
    }
}
