using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace ReceiptVault
{
    /**
     * This class'll read an image using OCR, and saves it in its field "scannedText"
     */

    internal class ImageScan
    {
        private readonly newEntryScreen form;
        private readonly int[,] position;
        private string scannedText;

        /// <summary>
        /// Note: Andere constructor dan in klasse diagram.
        /// </summary>
        /// <param name="position">
        ///     A two dementional array with the x- and y coordinates of the top left corner and the bottom
        ///     right corner.
        /// </param>
        /// <param name="file"></param>
        public ImageScan(int[,] position, StorageFile file, newEntryScreen form)
        {
            scannedText = "";
            this.form = form;

            this.position = position;
            //Debug.WriteLine("position size: " + position.Length);

            Debug.WriteLine("position content:");
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    Debug.WriteLine(this.position[i, j]);
                }
            }

            recognize(this.position, file);
        }

        public async void recognize(int[,] position, StorageFile file)
        {
            var ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("nl"));

            //note: testen van ocr, please delete.
            //var f = await Package.Current.InstalledLocation.GetFileAsync(@"Assets\testBonnetje.bmp");

            //hier de file gaan croppen.
            double scale = 1;

            var diffWidth = Math.Abs(position[1, 0] - position[0, 0]);
            var diffHeight = Math.Abs(position[1, 1] - position[0, 1]);
            var corpSize = new Size(diffWidth, diffHeight);

            // Convert start point and size to integer. 
            var startPointX = (uint) Math.Floor(position[0, 0]*scale);
            var startPointY = (uint) Math.Floor(position[0, 1]*scale);
            var height = (uint) Math.Floor(Math.Abs(corpSize.Height*scale));
            var width = (uint) Math.Floor(Math.Abs(corpSize.Width*scale));

            StorageFile croppedImage;

            using (IRandomAccessStream stream = await file.OpenReadAsync())
            {
                // Create a decoder from the stream. With the decoder, we can get  
                // the properties of the image. 
                var decoder = await BitmapDecoder.CreateAsync(stream);

                // The scaledSize of original image. 
                var scaledWidth = (uint) Math.Floor(decoder.PixelWidth*scale);
                var scaledHeight = (uint) Math.Floor(decoder.PixelHeight*scale);

                // Refine the start point and the size.  
                if (startPointX + width > scaledWidth)
                {
                    startPointX = scaledWidth - width;
                }


                if (startPointY + height > scaledHeight)
                {
                    startPointY = scaledHeight - height;
                }

                // Create cropping BitmapTransform and define the bounds. 
                var transform = new BitmapTransform();
                var bounds = new BitmapBounds();
                bounds.X = startPointX;
                bounds.Y = startPointY;
                bounds.Height = height;
                bounds.Width = width;
                transform.Bounds = bounds;

                transform.ScaledWidth = scaledWidth;
                transform.ScaledHeight = scaledHeight;

                byte[] receipt;
                try
                {
                    // Get the cropped pixels within the bounds of transform. 
                    var pix = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        transform,
                        ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.ColorManageToSRgb);
                    receipt = pix.DetachPixelData();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    var dialog =
                        new MessageDialog("Er is geen tekst herkent, zorg dat het bonnetje duidelijk op de foto staat.");
                    await dialog.ShowAsync();
                    return;
                }

                //foreach (byte t in pixels)
                //{
                //    Debug.WriteLine("Pixel found, value: " + t);
                //}

                // Stream the bytes into a WriteableBitmap 
                var cropBmp = new WriteableBitmap((int) width, (int) height);
                var pixStream = cropBmp.PixelBuffer.AsStream();
                pixStream.Write(receipt, 0, (int) (width*height*4));


                croppedImage = await WriteableBitmapToStorageFile(cropBmp);
                Debug.WriteLine("Imagecropped! " + croppedImage);
            }

            //ocr gedeelte:
            using (var stream = await croppedImage.OpenAsync(FileAccessMode.Read))
            {
                //  Debug.WriteLine("dit werkt");
                // Create image decoder.
                var decoder = await BitmapDecoder.CreateAsync(stream);

                // Load bitmap.
                var bitmap = await decoder.GetSoftwareBitmapAsync();

                // Extract text from image.
                var result = await ocrEngine.RecognizeAsync(bitmap);

                scannedText = result.Text;

                //de gescande tekst een tekstbox zetten.
                form.total = scannedText;

                if (result.Text == "")
                {
                    Debug.WriteLine("Er is geen tekst herkent, zorg dat het bonnetje duidelijk op de foto staat.");
                    var dialog =
                        new MessageDialog("Er is geen tekst herkent, zorg dat het bonnetje duidelijk op de foto staat.");
                    await dialog.ShowAsync();
                }
                else
                {
                    // Return recognized text.
                    Debug.WriteLine(result.Text);
                }
            }
        }

        public string getScannedText()
        {
            return scannedText;
        }

        private async Task<StorageFile> WriteableBitmapToStorageFile(WriteableBitmap WB)
        {
            var FileName = "receipts.";
            FileName += "bmp";
            var BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;

            var file =
                await
                    ApplicationData.Current.TemporaryFolder.CreateFileAsync(FileName,
                        CreationCollisionOption.GenerateUniqueName);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                var pixelStream = WB.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) WB.PixelWidth,
                    (uint) WB.PixelHeight,
                    96.0,
                    96.0,
                    pixels);
                await encoder.FlushAsync();
            }
            return file;
        }
    }
}