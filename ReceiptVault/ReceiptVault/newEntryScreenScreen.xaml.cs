using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
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
        public newEntryScreen()
        {
            this.InitializeComponent();
            testPic.Source = new BitmapImage(new Uri(base.BaseUri, "Assets/testBonnetje.bmp"));
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            /**   
               CameraCaptureUI captureUI = new CameraCaptureUI();
               captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
               captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

               StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

               testPic.Source = new BitmapImage(new Uri(base.BaseUri, "Assets/StoreLogo.png"));

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
             **/


            CameraCaptureUI dialog = new CameraCaptureUI();
            Size aspectRatio = new Size(16, 9);
            dialog.PhotoSettings.CroppedAspectRatio = aspectRatio;

            StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);

            testPic.Source = file;




        }

    }
}
