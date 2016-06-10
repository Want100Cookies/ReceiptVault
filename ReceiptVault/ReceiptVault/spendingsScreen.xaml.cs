using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
            if (comboBoxYear.SelectedIndex == 0)
            {
                comboBoxYear.SelectedIndex = comboBoxYear.Items.Count - 1;
            }

            //todo: fix alle nullreferenceExceptions...
           // Debug.WriteLine(comboBoxYear.select);


            DateTime beginDateTime;
            DateTime endDateTime;
            
            Debug.WriteLine("De selectedIndex is " + comboBoxWeek.SelectedIndex);

            //de gebruiker wil alleen per jaar...
            if (comboBoxWeek.SelectedIndex == 0 && comboBoxMonth.SelectedIndex == 0)
            {
                beginDateTime = new DateTime((int) comboBoxYear.SelectedValue, 1, 1);
                endDateTime = new DateTime((int) comboBoxYear.SelectedValue, 12, 31);
                Debug.WriteLine("Er gaat straks een getEntry methode aangeroepen worden.");
                Debug.WriteLine("De beginDate is: " + beginDateTime.ToString());
                Debug.WriteLine("De eindDate is: " + beginDateTime.ToString());
            } else if (comboBoxWeek.SelectedIndex != 0)
            {
                Debug.WriteLine(comboBoxYear.SelectedIndex);
                //beginDateTime = FirstDateOfWeek((int) comboBoxYear.SelectedValue,
                //    (int) comboBoxWeek.SelectedValue, CalendarWeekRule.FirstDay);
                //endDateTime = beginDateTime.AddDays(6);
                //Debug.WriteLine("Er gaat straks een getEntry methode aangeroepen worden.");
                //Debug.WriteLine("De beginDate is: " + beginDateTime.ToString());
                //Debug.WriteLine("De eindDate is: " + beginDateTime.ToString());
            }
        }

        static DateTime FirstDateOfWeek(int year, int weekNum, CalendarWeekRule rule)
        {
            Debug.Assert(weekNum >= 1);

            DateTime jan1 = new DateTime(year, 1, 1);

            int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            DateTime firstMonday = jan1.AddDays(daysOffset);
            Debug.Assert(firstMonday.DayOfWeek == DayOfWeek.Monday);

            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstMonday, rule, DayOfWeek.Monday);

            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            DateTime result = firstMonday.AddDays(weekNum * 7);

            return result;
        }

        /// <summary>
        /// Method that initializes all the dropdowns with the appropiate data.
        /// </summary>
        private void initDropdowns()
        {
            comboBoxMonth.Items.Add("Maand");
            comboBoxMonth.SelectedIndex = 0;


            comboBoxWeek.Items.Add("Week");
            comboBoxWeek.SelectedIndex = 0;

            comboBoxYear.Items.Add("Jaar");
            comboBoxYear.SelectedIndex = 0;
            //months/weeks:
            for (int i = 1; i <= 12; i++)
            {
                CheckBox c = new CheckBox();
                c.Content = i.ToString();
                comboBoxMonth.Items.Add(c);

                //for some reason wil hij dat er een nieuwe aangemaakt moet worden.
                c = new CheckBox();
                c.Content = i.ToString();
                comboBoxWeek.Items.Add(c);
            }

            //weeks, starting from 12 for saving the CPU cycles.
            for (int i = 13; i <= 52; i++)
            {
                CheckBox c = new CheckBox();
                c.Content = i.ToString();
                comboBoxWeek.Items.Add(c);
            }

            for (int i = 1950; i <= DateTime.Now.Year; i++)
            {
                CheckBox c = new CheckBox();
                c.Content = i.ToString();
                comboBoxYear.Items.Add(c);
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

        /// <summary>
        /// Button die alle input velden reset.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            comboBoxMonth.SelectedIndex = comboBoxWeek.SelectedIndex = comboBoxYear.SelectedIndex = 0;
        }

        private void ComboBoxWeek_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterDate();
        }
    }
}
