using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReceiptVault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class newEntryScreen : Page
    {
        private int[] dragStartPos;
        private int[] dragFinishPos;
        private EntryStore.Entry entry;

        //note: deze staat niet in de class diagram, maar is wel nodig:
        private Boolean dragging;
        private StorageFile photo;

        //i'll go to ... for this.
        public byte[] receipt;

        //field to edit the value of textBoxTotal.
        public string total
        {
            get { return textBoxTotal.Text; }
            set { textBoxTotal.Text = value; }
        }

        public newEntryScreen()
        {
            this.InitializeComponent();
            Debug.WriteLine("Het werkt");

            //note: dragStartPos = dragFinishPos = new int[2]; is nooit een goed idee geweest.
            dragStartPos = new int[2];
            dragFinishPos = new int[2];

            picker.Date = DateTime.Now;
        }

        /// <summary>
        /// note: deze staat niet in de class diagram
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

        public void acceptEntry()
        {

        }

        /// <summary>
        /// This method used to take a picture from the webcam, and save it to a picturebox.
        /// </summary>
        public async void takePicture()
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                //User cancelled photo capture
                Debug.WriteLine("photo = null");
                return;
            }

            //image saven:
            //Debug.WriteLine("image proberen te saven in " + ApplicationData.Current.LocalFolder);
            //note: dit gaan linken met iets in de db.
            //    await photo.CopyAsync(ApplicationData.Current.LocalFolder, "receipt.jpeg", NameCollisionOption.GenerateUniqueName);
            //   this.photo = photo;
            //  Debug.WriteLine("image saved");
            
            receipt = await ReadFile(photo);

              IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap,
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied);

            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);
           // imgNewReceipt.Source = bitmapSource;

            imgNewReceipt.Source = await ImageFromBytes(receipt);

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
                //Debug.WriteLine("drag move");
                dragFinishPos[0] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.X);
                dragFinishPos[1] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.Y);

                //Debug.WriteLine("moving... x, y: " + dragFinishPos[0] + ", " + dragFinishPos[1]);
                
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

            //note: deze is best pijnlijk...
            ImageScan scan = new ImageScan(new int[2,2] { {dragStartPos[0], dragStartPos[1] }, {dragFinishPos[0], dragFinishPos[1]} }, photo, this);

            //note: dit gebeurt niet, denk dat de methode te vroeg wordt aangeroepen...
            textBoxTotal.Text = scan.getScannedText();

            //receipt = scan.GetReceipt();
           // Debug.WriteLine(receipt.ToString());
            Debug.WriteLine("Invoerveld in aangepast nu.");
        }

        public void homeClicked()
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        public void spendingsClicked()
        {

        }

        public void VATClicked()
        {

        }

        private void buttonAccept_Click(object sender, RoutedEventArgs e)
        {
           // DateTime dt = Convert.ToDateTime(picker.Date);
            Debug.WriteLine(picker.Date.GetType());
            DateTimeOffset date = (DateTimeOffset) picker.Date;
            DateTime dt = date.DateTime;

            //note: picture encoding here.

            Debug.WriteLine("--------------");
            Debug.WriteLine("Receipt is op dit moment: " + receipt.GetType());
            Debug.WriteLine("--------------");

            Debug.WriteLine(dt.GetType());

           // Executing: insert into "Entry"("StoreName", "Total", "VATpercentage", "Date", "Receipt") values(?,?,?,?,?)

            entry = new EntryStore.Entry
            {
                StoreName = textBoxShopName.Text,
                Total = double.Parse(textBoxTotal.Text),
                VATpercentage = Int32.Parse(textBoxVAT.Text),
                Date = dt,
                Receipt = receipt
            };

            Debug.WriteLine(entry.StoreName);
            EntryStore.Instance.SaveEntry(entry);
            foreach (EntryStore.Entry enry in EntryStore.Instance.RetrieveEntry())
            {
                Debug.WriteLine(enry.Id);
            }
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

        private void textBlockHome_Tapped(object sender, TappedRoutedEventArgs e)
        {
            homeClicked();
        }

    }
}
