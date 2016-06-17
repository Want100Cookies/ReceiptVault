using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReceiptVault
{
    public sealed partial class newEntryScreen : Page
    {
        private readonly int[] dragStartPos;
        private readonly int[] dragFinishPos;

        //field die bijhoud of er een nieuwe entry is toegevoegd aan de database, op het moment dat er dan 
        //genavigeerd wordt kan er dan een argument worden meegegeven die zorgt dat de homescreen wordt geupdated.
        private bool isNewEntryAdded;

        //note:
        //het private field EntryStore.Entry entry hoeft niet als field gedefinieerd te worden, deze wordt dus lokaal gebruikt nu.
        
        //note: deze staat niet in de class diagram, maar is wel nodig:
        //is op dit moment de gebruiker aan het draggen? zo ja, pas het rode rechthoekje dan aan.
        private bool dragging;

        //De foto die gemaakt wordt.
        private StorageFile photo;

        //De imagepath naar de foto van het bonnetje.
        private string receipt;

        //field to edit the value of textBoxTotal, heeft de ImageScan nodig.
        public string total
        {
            get { return textBoxTotal.Text; }
            set { textBoxTotal.Text = value; }
        }

        public newEntryScreen()
        {
            this.InitializeComponent();

            //note: dit is nodig, anders blijf isNewEntryAdded null.
            isNewEntryAdded = false;

            //note: dragStartPos = dragFinishPos = new int[2]; is nooit een goed idee geweest.
            dragStartPos = new int[2];
            dragFinishPos = new int[2];

            picker.Date = DateTime.Now;

            //Dit heeft te maken met de taal waarin de OCR moet gaan uitlezen.
            System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("nl-NL");
        }

        /// <summary>
        /// note: deze staat niet in de class diagram
        /// Deze methode wordt gebruikt om het rechthoekje te tekenen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompositionTarget_Rendering(object sender, object args)
        {
            //Used for rendering the cropping rectangle on the image. 
            rect.SetValue(Canvas.LeftProperty, (dragStartPos[0] < dragFinishPos[0]) ? dragStartPos[0] : dragFinishPos[0]);
            rect.SetValue(Canvas.TopProperty, (dragStartPos[1] < dragFinishPos[1]) ? dragStartPos[1] : dragFinishPos[1]);
            
            rect.Width = (int) Math.Abs(dragFinishPos[0] - dragStartPos[0]);
            rect.Height = (int) Math.Abs(dragFinishPos[1] - dragStartPos[1]);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            takePicture();
        }

        /// <summary>
        /// Methode die wordt aangeroepen wanneer er op de accept button wordt geklikt, tevens wordt hierin ook alle input afgehandeld.
        /// </summary>
        public async void acceptEntry()
        {
            //alle input handling gebeurt hier
            MessageDialog dialogError = null;

            if (textBoxTotal.Text.Trim().Equals(""))
            {
                dialogError = new MessageDialog("Er ging hier iets mis, vul a.u.b. het totaal goed in.");
            }

            if (textBoxShopName.Text.Trim().Equals(""))
            {
                dialogError = new MessageDialog("Er ging hier iets mis, vul a.u.b. de winkel naam goed in.");
            }

            try
            {
                if (textBoxVAT.Text.Trim().Equals("") || int.Parse(textBoxVAT.Text) > 99 || int.Parse(textBoxVAT.Text) < 0)
                {
                    dialogError = new MessageDialog("Er ging hier iets mis, vul a.u.b. het btw bedrag goed in.");
                }
            }
            catch (Exception)
            {
                dialogError = new MessageDialog("Er ging hier iets mis, vul a.u.b. het btw bedrag goed in.");
            }

            //dt is verplicht tot initialisatie.
            DateTime dt = DateTime.Now;
            if (picker.Date == null)
            {
                dialogError = new MessageDialog("Er ging hier iets mis, vul a.u.b. het btw bedrag goed in.");
            }
            else
            {
                DateTimeOffset date = (DateTimeOffset) picker.Date;
                dt = date.DateTime;

                //er wordt een bonnetje uit de toekomst aangemaakt.
                if (dt.CompareTo(DateTime.Now) > 0)
                {
                    dialogError = new MessageDialog("Er ging hier iets mis, vul a.u.b. geen tijd in de toekomst in.");
                }
            }

            //is het nodig om een error terug te geven aan de gebruiker?
            if (dialogError != null)
            {
                await dialogError.ShowAsync();
                return;
            }  
            
            // Executing: insert into "Entry"("StoreName", "Total", "VATpercentage", "Date", "Receipt") values(?,?,?,?,?)

            try
            {
                EntryStore.Entry entry = new EntryStore.Entry
                {
                    StoreName = textBoxShopName.Text.Trim(),
                    Total = double.Parse(textBoxTotal.Text),
                    VATpercentage = Int32.Parse(textBoxVAT.Text),
                    Date = dt,
                    Receipt = receipt
                };

                EntryStore.Instance.SaveEntry(entry);
                isNewEntryAdded = true;

                textBoxShopName.Text = textBoxTotal.Text = textBoxVAT.Text = "";
                picker.Date = DateTime.Now;
            }
            catch (Exception)
            {
                //er ging wat echt mis met iets als het parsen.
                var error = new MessageDialog("Er is wat mis gegaan met het opslaan van de gegevens, zijn alle invoer velden correct ingevuld?");
                await error.ShowAsync();
                return;
            }
   
            var dialog = new MessageDialog("Het nieuwe bonnetje is succesvol opgeslagen.");
            await dialog.ShowAsync();
        }

        /// <summary>
        /// This method used to take a picture from the webcam, and save it to a picturebox.
        /// </summary>
        public async void takePicture()
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(imgNewReceipt.Width, imgNewReceipt.Height);

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                //User cancelled photo capture
                return;
            }

            //image saven:
            var storageFile = await photo.CopyAsync(ApplicationData.Current.LocalFolder, "receipt.jpeg", NameCollisionOption.GenerateUniqueName);
            Debug.WriteLine("De imagepath van de localstorage is: " + storageFile.Path.ToString());
            this.photo = photo;

            receipt = storageFile.Path;

            imgNewReceipt.Source = await ImageFromBytes(await ReadFile(photo));

            imgArrowFoto.Visibility = Visibility.Collapsed;
            imgArrowToPicture.Visibility = Visibility.Visible;
            TextBlockFeedback.Text = "Mooi, trek nu met uw muis een rechthoek \r\nom het totaalbedrag.";
        }

        /// <summary>
        /// Loads the byte data from a StorageFile
        /// </summary>
        /// <param name="file">The file to read</param>
        public async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes = null;

            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            { 
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }
    

        private static async Task<BitmapImage> LoadImage(StorageFile file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);

            bitmapImage.SetSource(stream);

            return bitmapImage;
        }

        /// <summary>
        /// Wanneer de user klikt op de picturebox en start met draggen.
        /// note: de volgende 3 methodes hebben te maken met draggen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void dragStart(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("dragStart");
            dragging = true;

            dragStartPos[0] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.X);
            dragStartPos[1] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.Y);

            dragFinishPos[0] = dragStartPos[0];
            dragFinishPos[1] = dragStartPos[1];

            rect.Visibility = Visibility.Visible;

            Debug.WriteLine("dragStartPost: " + dragStartPos[0].ToString() + ", " + dragStartPos[1].ToString());
        }

        public void dragMove(object sender, PointerRoutedEventArgs e)
        {
            if (dragging)
            {
                dragFinishPos[0] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.X);
                dragFinishPos[1] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.Y);
                
                CompositionTarget_Rendering(sender, null);
            }

        }

        public void dragFinish(object sender, PointerRoutedEventArgs e)
        {
            dragging = false;
            Debug.WriteLine("dragFinish");

            dragFinishPos[0] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.X);
            dragFinishPos[1] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.Y);

            Debug.WriteLine("dragFinishPos: " + dragFinishPos[0].ToString() + ", " + dragFinishPos[1].ToString());

            ImageScan scan = new ImageScan(new int[2,2] { {dragStartPos[0], dragStartPos[1] }, {dragFinishPos[0], dragFinishPos[1]} }, photo, this);

            textBoxTotal.Text = scan.getScannedText();

            //stukje user interaction.
            imgArrowToPicture.Visibility = Visibility.Collapsed;
            imgArrowInput.Visibility = Visibility.Visible;
            TextBlockFeedback.Text = "Goed, vul nu de benodige gegevens in.";
            
        }

        /// <summary>
        /// de volgende 6 methodes zijn voor het menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            this.Frame.Navigate(typeof(MainPage), isNewEntryAdded.ToString());
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

        private void buttonAccept_Click(object sender, RoutedEventArgs e)
        {
            acceptEntry();
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

        /// <summary>
        /// Stukje autosuggest voor de winkels:
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AutoSuggestBoxStoreName_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ObservableCollection<string> filteredStores = new ObservableCollection<string>();

                //Set the ItemsSource to be your filtered dataset
                string[] stores = EntryStore.Instance.getAllStoreNames(); //return original data from Store
                
                if (!string.IsNullOrEmpty(sender.Text))
                {
                    foreach (String storeName in stores)
                    {
                        if (storeName.ToLower().Contains(sender.Text.ToLower()))
                        {
                            filteredStores.Add(storeName);
                        }
                    }
                }
                else
                {
                    foreach (string storeName in EntryStore.Instance.getAllStoreNames())
                    {
                        filteredStores.Add(storeName);
                    }
                }

                sender.ItemsSource = filteredStores;
            }
        }

        /// <summary>
        /// Stukje autosuggest voor het btw bedrag:
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AutoSuggestBoxVAT_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ObservableCollection<int> filteredVatPercentages = new ObservableCollection<int>();

                //Set the ItemsSource to be your filtered dataset
                int[] vatPercentages = EntryStore.Instance.getAllVatPercentages(); //return original data from Store

                if (!string.IsNullOrEmpty(sender.Text))
                {
                    foreach (int vatPercentage in vatPercentages)
                    {
                        if (vatPercentage.ToString().Contains(sender.Text))
                        {
                            filteredVatPercentages.Add(vatPercentage);
                        }
                    }
                }
                else
                {
                    foreach (int vatPercentage in EntryStore.Instance.getAllVatPercentages())
                    {
                        filteredVatPercentages.Add(vatPercentage);
                    }
                }

                sender.ItemsSource = filteredVatPercentages;
            }
        }
    }
}
