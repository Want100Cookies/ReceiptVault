using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.WinRT;

namespace ReceiptVault
{
    public class EntryStore
    {
        public EntryStore()
        {
            using (var db = DbConnection)
            {
                //db.TraceListener = new DebugTraceListener();
                var c = db.CreateTable<Entry>();
                //var info = db.GetMapping(typeof(Entry));
                //Debug.WriteLine(info.Columns);
            }
        }

        private static SQLiteConnection DbConnection => new SQLiteConnection(
            new SQLitePlatformWinRT(), 
            Path.Combine(ApplicationData.Current.LocalFolder.Path, "Storage.sqlite"));

        public bool SaveEntry(Entry entry)
        {
            using (var db = DbConnection)
            {
                db.TraceListener = new DebugTraceListener();

                if (entry.Id != default(int))
                {
                    var s = db.Update(entry);
                    return s == 1;
                }
                else
                {
                    var s = db.Insert(entry);
                    return s == 1;
                }
            }
        }

        public Entry RetrieveEntry(int Id)
        {
            using (var db = DbConnection)
            {
                db.TraceListener = new DebugTraceListener();

                return db.Table<Entry>().First(v => v.Id.Equals(Id));
            }
        }

        public Entry[] RetrieveEntry(DateTime startDate, DateTime endDate)
        {
            using (var db = DbConnection)
            {
                db.TraceListener = new DebugTraceListener();

                return db.Table<Entry>().Where(s => s.Date >= startDate
                                                          && s.Date <= endDate).ToArray();
            }
        }

        public Entry[] RetrieveEntry()
        {
            using (var db = DbConnection)
            {
                db.TraceListener = new DebugTraceListener();

                return db.Table<Entry>().ToArray();
            }
        }

        public Entry[] RetrieveEntry(string storeName)
        {
            using (var db = DbConnection)
            {
                db.TraceListener = new DebugTraceListener();

                return db.Table<Entry>().Where(s => s.StoreName == storeName).ToArray();
            }
        }

        public class Entry
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string StoreName { get; set; }

            public double Total { get; set; }

            public int VATpercentage { get; set; }

            public DateTime Date { get; set; }

            public byte[] Receipt { get; set; }
        }
    }
}
