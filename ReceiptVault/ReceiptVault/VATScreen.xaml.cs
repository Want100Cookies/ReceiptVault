﻿using System;
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
        private bool initialized = false;


        public VATScreen()
        {
            this.InitializeComponent();
            startPicker.Date = DateTime.Now.AddDays(-30);
            endPicker.Date = DateTime.Now;
            filterStoreName();
            initialized = true;
            filterDate();
        }

        public void filterDate()
        {
            if (initialized)
            {
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

                DateTimeOffset date = (DateTimeOffset) startPicker.Date;
                DateTime startDateTime = date.DateTime;

                DateTimeOffset date2 = (DateTimeOffset) endPicker.Date;
                DateTime endDateTime = date2.DateTime;


                populateGraph(startDateTime, endDateTime, storeNames);
            }
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
        /// Data uit de database rippen aan de hand van begin- en endDates.
        /// </summary>
        private void populateGraph(DateTime beginDateTime, DateTime endDateTime, string[] storeNames)
        {
            Dictionary<String, List<EntryStore.Entry>> chartDictionary = new Dictionary<string, List<EntryStore.Entry>>();

            foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry(beginDateTime, endDateTime, storeNames))
            {
                //this looks very weird. Deze regel haalt de tijd weg bij de dateTime, op deze manier staat worden de uitgaven per dag op geteld (en niet per dag + tijdstip).
                entry.Date = entry.Date.Date;
                entry.Total = (entry.Total/100)*entry.VATpercentage;

                if (!chartDictionary.ContainsKey(entry.StoreName))
                {
                    //nee: maak een nieuwe list aan.
                    chartDictionary.Add(entry.StoreName, new List<EntryStore.Entry>());
                    //Debug.WriteLine("Er is een nieuwe key aangemaakt, " + entry.StoreName);
                }

                chartDictionary[entry.StoreName].Add(entry);

            }

            VATChart.Series.Clear();

            int i = 0;
            foreach (List<EntryStore.Entry> entries in chartDictionary.Values)
            {
                Debug.WriteLine(VATChart.Series.Count);

                VATChart.Series.Insert(i, new ColumnSeries());
                (VATChart.Series[i] as ColumnSeries).DependentValuePath = "Total";
                (VATChart.Series[i] as ColumnSeries).IndependentValuePath = "Date";

                //todo: elke chart series een vaste kleur: geel/oranje de kleur voor de jumbo... etc.

                (VATChart.Series[i] as ColumnSeries).Title = entries[0].StoreName;

                (VATChart.Series[i] as ColumnSeries).ItemsSource = entries;
                i++;
            }
        }

        private void ListBoxStores_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterDate();
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            filterStoreName();
        }

        private void StartPicker_OnDateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            filterDate();
        }

        #region side menu

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
            this.Frame.Navigate(typeof(MainPage), "False");
        }

        /// <summary>
        /// note: de volgende twee events zijn voor het veranderen van de mouse pointer wanneer er een hover plaatsvind over 1 van de 4 menu items.        
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlockHome_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor =
                new CoreCursor(CoreCursorType.Hand, 1);
        }

        private void TextBlockHome_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        #endregion


    }
}
