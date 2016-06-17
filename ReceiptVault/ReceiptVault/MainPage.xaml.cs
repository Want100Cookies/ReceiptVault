using System;
using System.Diagnostics;
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

namespace ReceiptVault
{
    public sealed partial class MainPage : Page
    {
        //note: een entries field is niet nodig geweest.

        public MainPage()
        {
            this.InitializeComponent();

            //er wordt gebruikt gemaakt van page caching, er worden dus pas gegevens uit de database gehaald pas op het moment dat het echt nodig is.
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        /// <summary>
        /// Updates all the entries on the screen, syncing the data of the database with the entries on the screen.
        /// </summary>
        public void updateEntries()
        {
            Debug.WriteLine("Entries worden geupdated omdat de update functie is aangeroepen.");

            panelReceipts.Children.Clear();

            //entries
            foreach (EntryStore.Entry entry in EntryStore.Instance.RetrieveEntry())
            {
                StackPanel panelEntry = new StackPanel();

                panelEntry.BorderThickness = new Thickness(1);

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
                txtStore.Width = panelEntry.Width - 55 - txtAmount.Text.Length*3;

                //finally: add to the panel.
                stackText.Children.Add(txtStore);
                stackText.Children.Add(txtAmount);

                panelEntry.Children.Add(stackText);

                panelReceipts.Children.Add(panelEntry);
            }
        }

        /// <summary>
        /// Als er op een entry gekikt wordt, moet het bonnetje in het groot verschijnen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void entryClicked(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = sender as StackPanel;
            Image img = stackPanel.Children[0] as Image;
            ImageSource source = img.Source;

            imageBigReceiptOverlay.Source = source;
            imageBigReceiptOverlay.Visibility = Visibility.Visible;
            imageClickOverlay.Visibility = Visibility.Visible;
        }


        #region side menu navigatie

        private void newRecieptClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (newEntryScreen));
        }

        private void VATClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (VATScreen));
        }

        private void spendingsClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (spendingsScreen));
        }

        private void homeClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (MainPage));
        }

        #endregion


        /// <summary>
        /// Event fired when navigating to this page.
        /// </summary>
        /// <param name="e">string met True of False of er geupdated moet worden (als er een nieuwe entry is toegevoegd) of niet (elke andere situatie.</param>
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

        /// <summary>
        /// De volgende 2 methodes zijn voor als de gebruiker wil het vergrootte bonnetje weg hebben.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
