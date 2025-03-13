using MockInterviewAI.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public static async Task<UserData> GetUserData(int userId)
        {
            return await _database.Table<UserData>().Where(x => x.UserId == userId).FirstOrDefaultAsync();
        }
    }
}
