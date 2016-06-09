using System;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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


        private void homeText_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
