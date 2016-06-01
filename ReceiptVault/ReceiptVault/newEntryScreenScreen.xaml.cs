using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public newEntryScreen()
        {
            this.InitializeComponent();
            Debug.WriteLine("Het werkt");

            dragStartPos = dragFinishPos = new int[2];
        }

        /// <summary>
        /// note: deze staat niet in de class diagram
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompositionTarget_Rendering(object sender, object args)
        {
            //Used for rendering the cropping rectangle on the image. 
            //Used for rendering the cropping rectangle on the image. 
            rect.SetValue(Canvas.LeftProperty, (dragStartPos[0] < dragFinishPos[0]) ? dragStartPos[0] : dragFinishPos[0]);
            rect.SetValue(Canvas.TopProperty, (dragStartPos[1] < dragFinishPos[1]) ? dragStartPos[1] : dragFinishPos[1]);
            rect.Width = (int)Math.Abs(dragFinishPos[0] - dragStartPos[0]);
            rect.Height = (int)Math.Abs(dragFinishPos[1] - dragStartPos[1]);
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
                // User cancelled photo capture
                System.Diagnostics.Debug.WriteLine("photo = null");
                return;
            }

            IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap,
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied);

            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);
            imgNewReceipt.Source = bitmapSource;
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

            Debug.WriteLine("dragStartPost: " + dragStartPos[0].ToString() + ", " +dragStartPos[1].ToString());
        }

        public void dragFinish(object sender, PointerRoutedEventArgs e)
        {
            dragging = false;
            Debug.WriteLine("dragFinish");

            dragFinishPos[0] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.X);
            dragFinishPos[1] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.Y);

            Debug.WriteLine("dragFinishPos: " + dragFinishPos[0].ToString() + ", " + dragFinishPos[1].ToString());

        }

        public void dragMove(object sender, PointerRoutedEventArgs e)
        {
            if (dragging)
            {
                //Debug.WriteLine("drag move");
                dragFinishPos[0] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.X);
                dragFinishPos[1] = Convert.ToInt32(e.GetCurrentPoint(imgNewReceipt).Position.Y);

                CompositionTarget_Rendering(sender, null);
            }

        }

        public void homeClicked()
        {

        }

        public void spendingsClicked()
        {

        }

        public void VATClicked()
        {

        }
    }
}
