using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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

            //note: wie kwam op dit idee?
            entries = new List<EntryStore.Entry>();

            NavigationCacheMode = NavigationCacheMode.Enabled;

           // updateEntries();
        }

        /// <summary>
        /// Updates all the entries on the screen, syncing the data of the database with the entries on the screen.
        /// </summary>
        public void updateEntries()
        {
            Debug.WriteLine("Entries worden geupdated omdat de update functie is aangeroepen.");
            //eerst entries op null zetten, wordt de garbage collector vrolijk van.
            entries = null;
            entries = new List<EntryStore.Entry>();

            panelReceipts.Children.Clear();

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

                panelEntry.Margin = new Thickness(0, 10, 20, 0);

                Image img = new Image();
                img.Height = panelEntry.Height - 20;
                if (entry.Receipt != null)
                {
                    panelEntry.Tapped += entryClicked;
                    panelEntry.PointerEntered += TextBlockHome_OnPointerEntered;
                    panelEntry.PointerExited += TextBlockHome_OnPointerExited;

                    img.Source = new BitmapImage(new Uri(entry.Receipt, UriKind.Absolute));
                }
                else
                {
                    img.Source = new BitmapImage(new Uri("ms-appx:/Assets/notFound.png"));
                }

                panelEntry.Children.Add(img);


                //storeName:
                StackPanel stackText = new StackPanel();
                stackText.Orientation = Orientation.Horizontal;
                stackText.Width = panelEntry.Width;
                    
                TextBlock txtAmount = new TextBlock();
                txtAmount.Text = entry.Total.ToString("'€'########.00");

                TextBlock txtStore = new TextBlock();
                txtStore.Text = entry.StoreName;

                //uitlijning
                txtStore.Width = panelEntry.Width - 55 - txtAmount.Text.Length;

                //finally: add to the panel.
                stackText.Children.Add(txtStore);
                stackText.Children.Add(txtAmount);

                panelEntry.Children.Add(stackText);

                panelReceipts.Children.Add(panelEntry);
            }
            
       }

        /// <summary>
        /// Is de lijst veranderd?
        /// note: TODO: DEZE WERKT NIET (helemaal). TODO
        /// </summary>
        /// <returns></returns>
        private Boolean isChanged()
        {
            //op het moment dat er meer of minder entries in de db zitten, moet er per definitie een sync plaatsvinden.
            if (EntryStore.Instance.RetrieveEntry().Length != entries.Count)
            {
                return true;
            }

            //note: op dit punt in de code weten we 100% zeker dat beide data sources (scherm en slLite database) dezelfde grootte hebben.
            EntryStore.Entry[] entriesInDB = EntryStore.Instance.RetrieveEntry();

            int[] idsInDB = new int[entriesInDB.Length];
            int[] idsOnScreen = new int[entries.Count];
            
            //array ids vullen met alle id's uit de database.
            for (int i = 0; i < entriesInDB.Length; i++)
            {
                EntryStore.Entry entry = entriesInDB[i];
                idsInDB[i] = entry.Id;
                Debug.WriteLine("Entrystore heeft het volgende id: " + idsInDB[i]);
            }

            //array idsOnScreen vullen met alle id's van de receipts op het scherm.
            for (int i = 0; i < idsInDB.Length; i++)
            {
                EntryStore.Entry entry = entries[i];
                idsOnScreen[i] = entry.Id;
                Debug.WriteLine("Plaatje op het scherm heeft het volgende id: " + idsOnScreen[i]);
            }

            Debug.WriteLine("Conclusie: Zijn de 2 veranderd? " + !Enumerable.SequenceEqual(idsInDB, idsOnScreen));

            return !Enumerable.SequenceEqual(idsInDB, idsOnScreen);
        }


        private void entryClicked(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;
            Image img = stackPanel.Children[0] as Image;
            ImageSource source = img.Source;

            imageBigReceiptOverlay.Source = source;
            imageBigReceiptOverlay.Visibility = Visibility.Visible;
            imageClickOverlay.Visibility = Visibility.Visible;

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
        /// Event fired when navigating to this page.
        /// </summary>
        /// <param name="e">need to update?</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string text = e.Parameter as string;
            Debug.WriteLine("Navigated to homepage, arguments received: " + text);
            if (text != null)
            {
                Debug.WriteLine("Type: " + text.GetType());

                if (text.Equals("True"))
                {
                    updateEntries();
                }
                else if (text.Equals(""))
                {
                    //de app wordt voor de eerste keer gestart:
                    Debug.WriteLine("De app wordt voor de eerste keer gestart");
                    updateEntries();
                }
            }
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

        public async static Task<BitmapImage> ImageFromBytes(Byte[] bytes)
        {
            BitmapImage image = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(bytes.AsBuffer());
                stream.Seek(0);
                await image.SetSourceAsync(stream);
            }
            return image;
        }

        private void ImageBigReceiptOverlay_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            imageBigReceiptOverlay.Visibility = Visibility.Collapsed;
            imageClickOverlay.Visibility = Visibility.Collapsed;
        }

        private void ImageClickOverlay_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            imageBigReceiptOverlay.Visibility = Visibility.Collapsed;
            imageClickOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
