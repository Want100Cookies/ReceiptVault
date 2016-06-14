using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
        public async void updateEntries()
        {
            Debug.WriteLine("isChanged() is " + isChanged());
            if (isChanged())
            {
                //eerst entries op null zetten, wordt de garbage collector vrolijk van.
                entries = null;
                entries = new List<EntryStore.Entry>();

                panelReceipts.Children.Clear();

             //   panelReceipts.Padding = new Thickness(20, 10, 0, 0);


                int i = 1;
                //entries
                foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry())
                {
                    entries.Add(entry);
                    //Debug.WriteLine("Entry found! " + entry.Id);
                    StackPanel panelEntry = new StackPanel();

                    panelEntry.BorderThickness = new Thickness(1);
                   // panelEntry.Background = new SolidColorBrush(Colors.Chocolate);

                    panelEntry.BorderBrush = new SolidColorBrush(Colors.Black);

                    panelEntry.Width = 250;
                    panelEntry.Height = 200;

                    panelEntry.Padding = new Thickness(3, 1, 3, 1);

                    panelEntry.Margin = new Thickness(20, 10, 0, 0);

                    //todo: image convertion here.
                    // foreach (byte b in entry.Receipt)
                    {
                        //       Debug.WriteLine("byte found: " + b);
                    }


                    Debug.WriteLine(entry.Receipt);
                    if (entry.Receipt != null)
                    {
                        Debug.WriteLine("--------------------");
                      //  Debug.WriteLine(await ImageFromBytes(entry.Receipt));

                        Debug.WriteLine(entry.Receipt.ToString());
                        Debug.WriteLine("--------------------");

                        //note: het verhaal over het opslaan van images laten we even.
                        Image img = new Image();
                        //File.WriteAllBytes("receipt.jpg", entry.Receipt);
                        img.Source = new BitmapImage(new Uri("ms-appx:///Assets/testBonnetje" + i + ".jpg"));
                        img.Height = panelEntry.Height - 20;
                        
                        panelEntry.Children.Add(img);
                    }

                    //storeName:
                    StackPanel stackText = new StackPanel();
                    stackText.Orientation = Orientation.Horizontal;
                    stackText.Width = panelEntry.Width;

                    TextBlock txtStore = new TextBlock();
                    txtStore.Text = entry.StoreName;
                    txtStore.Width = panelEntry.Width - 55;
                //    txtStore.Margin = new Thickness(0, 0, 100, 0);

                    TextBlock txtAmount = new TextBlock();
                    txtAmount.Text = entry.Total.ToString("'€'########.00");
                //    txtAmount.Margin = new Thickness(100, 0, 0, 0);

                    //finally: add to the panel.
                    stackText.Children.Add(txtStore);
                    stackText.Children.Add(txtAmount);

                    panelEntry.Children.Add(stackText);

                    panelReceipts.Children.Add(panelEntry);

                    i++;
                    if (i == 7)
                    {
                        i = 1;
                    }
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
                //Debug.WriteLine("Entrystore heeft het volgende id: " + idsInDB[i]);
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

        //public byte[] ImageToByte(Image image)
        //{
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        // Convert Image to byte[]
        //        image.Save(ms, format);
        //        byte[] imageBytes = ms.ToArray();
        //        return imageBytes;
        //    }
        //}
        ////public Image Base64ToImage(string base64String)
        //public Image ByteToImage(byte[] imageBytes)
        //{
        //    // Convert byte[] to Image
        //    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
        //    ms.Write(imageBytes, 0, imageBytes.Length);
        //    Image image = new Bitmap(ms);
        //    return image;
        //}
    }
}
