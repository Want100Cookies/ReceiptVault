using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.PointOfService;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace ReceiptVault
{
    /**
     * This class'll read an image using OCR, and saves it in its field "scannedText"
     */
    class ImageScan
    {
        private string scannedText;
        private int[,] position;

        private readonly newEntryScreen form;

        /// <summary>
        /// Note: Andere constructor dan in klasse diagram.
        /// </summary>
        /// <param name="position">A two dementional array with the x- and y coordinates of the top left corner and the bottom right corner.</param>
        /// <param name="file"></param>
        public ImageScan(int[,] position, StorageFile file, newEntryScreen form)
        {
            scannedText = "";
            this.form = form;

            this.position = position;
            //Debug.WriteLine("position size: " + position.Length);
            
            Debug.WriteLine("position content:");
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Debug.WriteLine(this.position[i, j]);
                }
            }

            recognize(this.position, file);

        }

        public async void recognize(int[,] position, StorageFile file)
        {
            var ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("nl"));

            //note: testen van ocr, please delete.
            //var f = await Package.Current.InstalledLocation.GetFileAsync(@"Assets\testBonnetje.bmp");

            //hier de file gaan croppen.
            double scale = 1;
            
            int diffWidth = Math.Abs(position[1, 0] - position[0, 0]);
            int diffHeight = Math.Abs(position[1, 1] - position[0, 1]);
            Size corpSize = new Size(diffWidth, diffHeight);

            // Convert start point and size to integer. 
            uint startPointX = (uint)Math.Floor(position[0, 0] * scale);
            uint startPointY = (uint)Math.Floor(position[0, 1] * scale);
            uint height = (uint)Math.Floor(Math.Abs(corpSize.Height * scale));
            uint width = (uint)Math.Floor(Math.Abs(corpSize.Width * scale));

            StorageFile croppedImage;

            using (IRandomAccessStream stream = await file.OpenReadAsync())
            {
                // Create a decoder from the stream. With the decoder, we can get  
                // the properties of the image. 
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // The scaledSize of original image. 
                uint scaledWidth = (uint)Math.Floor(decoder.PixelWidth * scale);
                uint scaledHeight = (uint)Math.Floor(decoder.PixelHeight * scale);

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
                BitmapTransform transform = new BitmapTransform();
                BitmapBounds bounds = new BitmapBounds();
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
                    PixelDataProvider pix = await decoder.GetPixelDataAsync(
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
                    var dialog = new MessageDialog("Er is geen tekst herkent, zorg dat het bonnetje duidelijk op de foto staat.");
                    await dialog.ShowAsync();
                    return;
                }

                //foreach (byte t in pixels)
                //{
                //    Debug.WriteLine("Pixel found, value: " + t);
                //}

                // Stream the bytes into a WriteableBitmap 
                WriteableBitmap cropBmp = new WriteableBitmap((int)width, (int)height);
                Stream pixStream = cropBmp.PixelBuffer.AsStream();
                pixStream.Write(receipt, 0, (int)(width * height * 4));
                

                croppedImage = await WriteableBitmapToStorageFile(cropBmp);
                Debug.WriteLine("Imagecropped! " + croppedImage);
            }

            //ocr gedeelte:
            using (var stream = await croppedImage.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
              //  Debug.WriteLine("dit werkt");
                // Create image decoder.
                var decoder = await BitmapDecoder.CreateAsync(stream);

                // Load bitmap.
                 var bitmap = await decoder.GetSoftwareBitmapAsync();
            
                // Extract text from image.
                OcrResult result = await ocrEngine.RecognizeAsync(bitmap);

                scannedText = result.Text;

                //de gescande tekst een tekstbox zetten.
                form.total = scannedText;
                
                if (result.Text == "")
                {
                    Debug.WriteLine("Er is geen tekst herkent, zorg dat het bonnetje duidelijk op de foto staat.");
                    var dialog = new MessageDialog("Er is geen tekst herkent, zorg dat het bonnetje duidelijk op de foto staat.");
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
            string FileName = "receipts.";
            FileName += "bmp";
            Guid BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;
            
            var file = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                    (uint)WB.PixelWidth,
                                    (uint)WB.PixelHeight,
                                    96.0,
                                    96.0,
                                    pixels);
                await encoder.FlushAsync();
            }
            return file;
        }
    }
}

