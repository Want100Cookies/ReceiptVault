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
                db.TraceListener = new DebugTraceListener();

                var c = db.CreateTable<Entry>();
                var info = db.GetMapping(typeof(Entry));

                Debug.WriteLine(info.Columns);
            }
        }

        private static SQLiteConnection DbConnection => new SQLiteConnection(
            new SQLitePlatformWinRT(), 
            Path.Combine(ApplicationData.Current.LocalFolder.Path, "Storage.sqlite"));

        internal class Entry
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
