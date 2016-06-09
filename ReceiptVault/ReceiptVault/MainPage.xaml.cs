using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls;
using Panel = Windows.Devices.Enumeration.Panel;

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

        /// <summary>
        /// Updates all the entries on the screen, syncing the data of the database with the entries on the screen.
        /// NOTE: dit werkt, maar de uitlijning is minder. TODO
        /// </summary>
        public void updateEntries()
        {
            Debug.WriteLine("isChanged() is " + isChanged());
            if (isChanged())
            {
                //eerst entries op null zetten, wordt de garbage collector vrolijk van.
                entries = null;
                entries = new List<EntryStore.Entry>();

                panelReceipts.Children.Clear();

             //   panelReceipts.Padding = new Thickness(20, 10, 0, 0);

                //entries
                foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry())
                {
                    entries.Add(entry);
                    //Debug.WriteLine("Entry found! " + entry.Id);
                    StackPanel panelEntry = new StackPanel();
                    //todo: image convertion here.
                    panelEntry.Children.Add(new Image());

                    //storeName:
                    TextBlock txtStore = new TextBlock();
                    txtStore.Text = entry.StoreName;

                    TextBlock txtAmount = new TextBlock();
                    txtAmount.Text = entry.Total.ToString("C");

                    //finally: add to the panel.
                    panelEntry.Children.Add(txtStore);
                    panelEntry.Children.Add(txtAmount);

                    panelEntry.BorderThickness = new Thickness(1);
                    panelEntry.Background = new SolidColorBrush(Colors.Chocolate);

                    panelEntry.BorderBrush = new SolidColorBrush(Colors.Red);

                    panelEntry.Width = 150;
                    panelEntry.Height = 100;

                    panelEntry.Margin = new Thickness(20, 10, 0, 0);
                    
                    panelReceipts.Children.Add(panelEntry);
                    
                }
            }
        }

        /// <summary>
        /// Is de lijst veranderd?
        /// TODO: DEZE WERKT NIET (helemaal). TODO
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
        //        Debug.WriteLine("Entrystore heeft het volgende id: " + idsInDB[i]);
            }

            //array idsOnScreen vullen met alle id's van de receipts op het scherm.
            for (int i = 0; i < idsInDB.Length; i++)
            {
                EntryStore.Entry entry = entriesInDB[i];
                idsInDB[i] = entry.Id;
         //       Debug.WriteLine("Plaatje op het scherm heeft het volgende id: " + idsInDB[i]);
            }

          //  Debug.WriteLine("Conclusie: zijn de 2 data sources gelijk? " + !Enumerable.SequenceEqual(idsInDB, idsOnScreen));

            return !Enumerable.SequenceEqual(idsInDB, idsOnScreen);
        }


        private void entryClicked(object sender, RoutedEventArgs e)
        {

        }

        private void newRecieptClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(newEntryScreen));
        }

        private void VATClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VATScreen));
        }

        private void spendingsClicked(object sender, RoutedEventArgs e)
        {
          //  this.Frame.Navigate(typeof(spendingScreen));
        }

        private void homeClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// note: de volgende twee events zijn voor het veranderen van de mouse pointer wanneer er een hover plaatsvind over 1 van de 4 menu items.        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlockHome_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
         Window.Current.CoreWindow.PointerCursor =
             new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void TextBlockHome_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }
    }
}
