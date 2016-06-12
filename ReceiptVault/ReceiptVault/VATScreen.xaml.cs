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

    //public class TestStore
    //{
    //    public String Name { get; set; }
    //    public int Amount { get; set; }

    //}

    public sealed partial class VATScreen : Page
    {

        public VATScreen()
        {
            this.InitializeComponent(); 
            Windows.UI.Xaml.Controls.ListBoxItem addStore = new Windows.UI.Xaml.Controls.ListBoxItem();
            addStore.Content = "dafg";
            listBox1.Items.Add(addStore);
            LoadChartContents();
            Debug.WriteLine("test completed");
            
        }

        private void LoadChartContents()
        {
            List <EntryStore.Entry> listSource = new List<EntryStore.Entry>();
            listSource.Add(new EntryStore.Entry() { VATpercentage = 21, Total = 20.23 });
                
            (VATChart.Series[0] as LineSeries).ItemsSource = listSource;
        }

        /// <summary>
        /// sidebar navigatie:
        /// </summary>
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
