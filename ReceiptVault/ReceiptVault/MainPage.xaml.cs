using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ReceiptVault
{
    public sealed partial class MainPage : Page
    {
        private List<EntryStore.Entry> entries;

        public MainPage()
        {
            this.InitializeComponent();

            entries = new List<EntryStore.Entry>();

            foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry())
            {
                entries.Add(entry);
                Debug.WriteLine("Entry found! " + entry.Id);
            }

            updateEntries();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //ImageScan scan = new ImageScan(new int[,] { {1,2}, {3,4} });
        }

        private void buttonNewReceipt_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(newEntryScreen));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VATScreen));
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (VATScreen));
        }

        /// <summary>
        /// Updates all the entries on the screen, syncing the data of the database with the entries on the screen.
        /// </summary>
        public void updateEntries()
        {
            if (!isChanged())
            {
                //eerst entries op null zetten, wordt de garbage collector vrolijk van.
                entries = null;
                entries = new List<EntryStore.Entry>();
                //entries
                foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry())
                {
                    entries.Add(entry);
                    Debug.WriteLine("Entry found! " + entry.Id);
                }
            }
        }

        /// <summary>
        /// Is de lijst veranderd?
        /// </summary>
        /// <returns></returns>
        private Boolean isChanged()
        {
            //op het moment dat er meer of minder entries in de db zitten, moet er per definitie een sync plaatsvinden.
            if (EntryStore.Instance.RetrieveEntry().Length != entries.Count)
            {
                return false;
            }

            //note: op dit punt in de code weten we 100% zeker dat beide data sources (scherm en slLite database) dezelfde grootte hebben.
            EntryStore.Entry[] entriesInDB = EntryStore.Instance.RetrieveEntry();

            int[] idsInDB = new int[entriesInDB.Length];
            int[] idsOnScreen = new int[entries.Count];
            
            //array ids vullen met alle id's uit de database.
            for (int i = 0; i < EntryStore.Instance.RetrieveEntry().Length; i++)
            {
                EntryStore.Entry entry = entriesInDB[i];
                idsInDB[i] = entry.Id;
                Debug.WriteLine("Entrystore heeft het volgende id: " + idsInDB[i]);
            }

            //array idsOnScreen vullen met alle id's van de receipts op het scherm.
            for (int i = 0; i < idsInDB.Length; i++)
            {
                EntryStore.Entry entry = entriesInDB[i];
                idsInDB[i] = entry.Id;
                Debug.WriteLine("Plaatje op het scherm heeft het volgende id: " + idsInDB[i]);
            }

            Debug.WriteLine("Conclusie: zijn de 2 data sources gelijk? " + Enumerable.SequenceEqual(idsInDB, idsOnScreen));

            return !Enumerable.SequenceEqual(idsInDB, idsOnScreen);
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
