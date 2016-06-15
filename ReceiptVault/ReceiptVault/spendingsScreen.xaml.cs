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
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReceiptVault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class spendingsScreen : Page
    {
        //variabele om te verzekeren dat eerst alle comboboxes zijn gevuld met waardes, voordat hier iets mee gedaan wordt.
        private Boolean initialized;

        public spendingsScreen()
        {
            this.InitializeComponent();
            initDropdowns();
            filterStoreName();
            filterDate();
        }

        /// <summary>
        /// Data uit de database rippen aan de hand van begin- en endDates.
        /// </summary>
        private void populateGraph(DateTime beginDateTime, DateTime endDateTime, string[] storeNames)
        {
            List<EntryStore.Entry> chartData = new List<EntryStore.Entry>();

            foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry(beginDateTime, endDateTime, storeNames))
            {
                
                //this looks very weird. Deze regel haalt de tijd weg bij de dateTime, op deze manier staat worden de uitgaven per dag op geteld (en niet per dag + tijdstip).
                entry.Date = entry.Date.Date;
                                chartData.Add(entry);
            }

            (spendingChart.Series[0] as ColumnSeries).ItemsSource = chartData;
            
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
                        CheckBox check = new CheckBox();
                        check.Content = storeName;
                        check.Unchecked += Check_Checked;
                        check.Checked += Check_Checked;
                        listBoxStores.Items.Add(check);
                    }
                }
            }
            else
            {
                foreach (string storeName in EntryStore.Instance.getAllStoreNames())
                {
                    CheckBox check = new CheckBox();
                    check.Content = storeName;
                    check.Unchecked += Check_Checked;
                    check.Checked += Check_Checked;
                    listBoxStores.Items.Add(check);
                }
            }
        }

        private void Check_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("event fired");
            filterDate();
        }

        /// <summary>
        /// filter de graph op begin- en einddatum.
        /// </summary>
        private void filterDate()
        {
            if (initialized)
            {
                if (comboBoxYear.SelectedIndex == 0)
                {
                    comboBoxYear.SelectedIndex = comboBoxYear.Items.Count - 1;
                }

                DateTime beginDateTime = new DateTime(Int32.Parse(comboBoxYear.Items[1].ToString()), 1, 1);
                DateTime endDateTime = DateTime.Now;

                Debug.WriteLine("De selectedIndex is " + comboBoxWeek.SelectedIndex);

                //de gebruiker wil alleen per jaar...
                if (comboBoxWeek.SelectedIndex == 0 && comboBoxMonth.SelectedIndex == 0 && comboBoxYear.SelectedIndex != 0)
                {
                    beginDateTime = new DateTime((int) comboBoxYear.SelectedValue, 1, 1, 0, 0, 0);
                    endDateTime = new DateTime((int) comboBoxYear.SelectedValue, 12, 31, 23, 59, 59);
                    Debug.WriteLine("Er gaat straks een getEntry methode aangeroepen worden.");
                    Debug.WriteLine("De beginDate is: " + beginDateTime);
                    Debug.WriteLine("De eindDate is: " + endDateTime);
                }
                else if (comboBoxWeek.SelectedIndex != 0)
                {
                    //de gebruiker wil alleen per week.
                    //note: dit moet nog getest worden.
                    Debug.WriteLine(comboBoxYear.SelectedIndex);
                    beginDateTime = FirstDateOfWeek((int) comboBoxYear.SelectedValue,
                        (int) comboBoxWeek.SelectedValue, CalendarWeekRule.FirstDay);
                    endDateTime = beginDateTime.AddDays(6);
                    Debug.WriteLine("Er gaat straks een getEntry methode aangeroepen worden.");
                    Debug.WriteLine("De beginDate is: " + beginDateTime);
                    Debug.WriteLine("De eindDate is: " + endDateTime);
                }
                else if (comboBoxMonth.SelectedIndex != 0)
                {
                    beginDateTime = new DateTime((int)comboBoxYear.SelectedValue, (int) comboBoxMonth.SelectedValue, 1, 0, 0, 0);
                    endDateTime = new DateTime((int) comboBoxYear.SelectedValue, 
                        (int) comboBoxMonth.SelectedValue, 
                        DateTime.DaysInMonth((int)comboBoxYear.SelectedValue, 
                        (int)comboBoxMonth.SelectedValue), 23, 59, 59);

                }
                
                EntryStore.Entry[] entries = EntryStore.Instance.RetrieveEntry(beginDateTime, endDateTime);
                foreach (EntryStore.Entry entry in entries)
                {
                    Debug.WriteLine("-------Entry----------");
                    Debug.WriteLine(entry.Id);
                    Debug.WriteLine(entry.Date.ToString());
                    Debug.WriteLine(entry.StoreName);
                    Debug.WriteLine(entry.Total);
                    Debug.WriteLine("-------/entry----------");
                }

                string[] storeNames = new string[listBoxStores.Items.Count];
                int i = 0;
                if (listBoxStores.Items != null)
                {
                    foreach (CheckBox checkBox in listBoxStores.Items)
                    {
                        if (checkBox.IsChecked != null && checkBox.IsChecked.Value)
                        {
                            Debug.WriteLine(checkBox.Content);
                            storeNames[i] = checkBox.Content.ToString();
                            i++;
                        }
                    }

                    //er is geen enkele winkel aangevinkt: alle data laten zien.
                    if (i == 0)
                    {
                        storeNames = EntryStore.Instance.getAllStoreNames();
                    }
                }

                populateGraph(beginDateTime, endDateTime, storeNames);

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

            initialized = true;
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
            filterStoreName();
        }

        private void ComboBoxWeek_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterDate();
        }

        private void ListBoxStores_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterDate();
        }
    }
}
