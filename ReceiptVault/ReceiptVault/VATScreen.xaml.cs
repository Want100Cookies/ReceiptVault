using System;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReceiptVault
{



    public sealed partial class VATScreen : Page
    {
        //List with entries
        List<EntryStore.Entry> listSource;

        public VATScreen()
        {
            listSource = new List<EntryStore.Entry>();
            this.InitializeComponent();
            loadListBox();
            LoadChartContents();
        }

        //Loads the date and VAT price in the chart
        private void LoadChartContents()
        {
            listSource.Add(new EntryStore.Entry() { VATpercentage = 21, Date = new DateTime(2015, 12, 12) });

            (VATChart.Series[0] as ColumnSeries).ItemsSource = listSource;
        }

        private void LoadChartContents2(DateTime beginDateTime, DateTime endDateTime)
        {
            

            foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry(beginDateTime, endDateTime))
            {
                //this looks very weird. Deze regel haalt de tijd weg bij de dateTime, op deze manier staat worden de uitgaven per dag op geteld (en niet per dag + tijdstip).
                entry.Date = entry.Date.Date;
                listSource.Add(entry);
            }

            (VATChart.Series[0] as ColumnSeries).ItemsSource = listSource;

        }





        //Loads all the store names in the listbox
        private void loadListBox()
        {
            foreach (String storenames in EntryStore.Instance.getAllStoreNames())
            {
                Windows.UI.Xaml.Controls.ListBoxItem addStore = new Windows.UI.Xaml.Controls.ListBoxItem();
                addStore.Content = storenames;
                listBox1.Items.Add(addStore);
            }
        }

        //Loads the menubar
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
            this.Frame.Navigate(typeof(spendingsScreen));
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
