using System;
using System.IO;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace Stacklash.iOS
{
    public enum HistoryItemType {
        Question,
        User
    }

    public class HistoryItem {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int ItemId { get; set; }
        public HistoryItemType Type { get; set; }
    }

    public class Database : SQLiteAsyncConnection
    {
        private static Database db;

        private static string GetPath() {
            return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "Local.db");
        }

        public Database() : base(GetPath())
        {
        }

        public static Task<List<HistoryItem>> GetHistory(HistoryItemType type) {
            return db.Table<HistoryItem>().Where(h => h.Type == type).OrderByDescending(h => h.Timestamp).Take(30).ToListAsync();
        }

        public static Task AddQuestionHistory(int questionId) {
            return db.InsertAsync(new HistoryItem { 
                ItemId = questionId, 
                Type = HistoryItemType.Question,
                Timestamp = DateTime.Now
            });
        }

        public static Task AddUserHistory(int userId) {
            return db.InsertAsync(new HistoryItem { 
                ItemId = userId, 
                Type = HistoryItemType.User,
                Timestamp = DateTime.Now
            });
        }

        public static Task Init() {

            db = new Database();

            NSFileManager.SetSkipBackupAttribute(GetPath(), true);

            var t = db.RunInTransactionAsync((SQLiteConnection c) => {
                    db.CreateTableAsync<HistoryItem>();
            });

            t.ConfigureAwait(false);

            return t;
        }
    }
}
