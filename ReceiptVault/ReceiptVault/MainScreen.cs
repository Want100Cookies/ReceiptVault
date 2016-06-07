using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;

namespace ReceiptVault
{
    
    class MainScreen
    {
        private List<EntryStore.Entry> entries;

        public MainScreen()
        {
            entries = new List<EntryStore.Entry>();

            foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry())
            {
                entries.Add(entry);
                Debug.WriteLine("Entry found! " + entry.Id);
            }
          //  entries = EntryStore.Instance.RetrieveEntry();

        }

        /// <summary>
        /// Updates all the entries on the screen, syncing the data of the database with the entries on the screen.
        /// </summary>
        public void updateEntries()
        {
            foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry())
            {
                entries.Add(entry);
                Debug.WriteLine("Entry found! " + entry.Id);
            }
        }

        public void entryClicked()
        {

        }

        public void newRecieptClicked()
        {

        }

        public void VATClicked()
        {

        }

        public void spendingsClicked()
        {
            
        }
    }
}
