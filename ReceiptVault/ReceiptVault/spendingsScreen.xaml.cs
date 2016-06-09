using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReceiptVault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class spendingsScreen : Page
    {
        public spendingsScreen()
        {
            this.InitializeComponent();
            filterStoreName();
            initDropdowns();
        }

        /// <summary>
        /// Data uit de database rippen.
        /// </summary>
        private void populateGraph()
        {
            
        }

        /// <summary>
        /// filter de listBoxStores op de inhoud van textBoxSearch.
        /// note: werkt goed, deze mag rik ook hebben.
        /// </summary>
        private void filterStoreName()
        {
            string[] stores = EntryStore.Instance.getAllStoreNames(); //return original data from Store

            listBoxStores.Items.Clear();

            if (!string.IsNullOrEmpty(textBoxSearch.Text))
            {
                foreach (String storeName in stores)
                {
                    if (storeName.ToLower().Contains(textBoxSearch.Text.ToLower()))
                    {
                        listBoxStores.Items.Add(storeName);
                    }
                }
            }
            else
            {
                foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry())
                {
                    string storeName = entry.StoreName;
                    listBoxStores.Items.Add(storeName);
                }
            }
        }
       
        /// <summary>
        /// filter de graph op begin- en einddatum.
        /// </summary>
        private void filterDate()
        {
           // EntryStore.Instance.RetrieveEntry()
        }

        /// <summary>
        /// Method that initializes all the dropdowns with the appropiate data.
        /// </summary>
        private void initDropdowns()
        {
            //months/weeks:
            for (int i = 1; i <= 12; i++)
            {
                comboBoxMonth.Items.Add(i);
                comboBoxWeek.Items.Add(i);
            }

            //weeks, starting from 12 for saving the CPU cycles.
            for (int i = 13; i <= 52; i++)
            {
                comboBoxWeek.Items.Add(i);
            }

            for (int i = 1950; i <= DateTime.Now.Year; i++)
            {
                comboBoxYear.Items.Add(i);
            }
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

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            filterStoreName();
        }
    }
}
