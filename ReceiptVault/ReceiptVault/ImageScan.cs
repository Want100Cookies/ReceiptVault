using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.PointOfService;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;


namespace ReceiptVault
{
    /**
     * This class'll read an image using OCR, and saves it in its field "scannedText"
     */
    class ImageScan
    {
        private String scannedText;
        private int[,] position;

        public ImageScan(int[,] position)
        {
            this.position = position;
            //Debug.WriteLine("position size: " + position.Length);
            //engine = new OcrEngine()

            foreach (var lang in Windows.Media.Ocr.OcrEngine.AvailableRecognizerLanguages)
            {
                //Debug.WriteLine(lang.DisplayName.ToString());
            }

            recognize(position);

        }

        public async void recognize(int[,] position)
        {
            var ocrEngine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("en"));

            // var file = await StorageFolder.GetFileAsync(Package.Current.InstalledLocation.Path + @"\Assets\testBonnetjePleaseDelete.bmp");

            var file = await Package.Current.InstalledLocation.GetFileAsync(@"Assets\testBonnetje.bmp");
           // Debug.WriteLine(file);

            //hier de file gaan croppen.


            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                Debug.WriteLine("dit werkt");
                // Create image decoder.
                var decoder = await BitmapDecoder.CreateAsync(stream);

                // Load bitmap.
                 var bitmap = await decoder.GetSoftwareBitmapAsync();
            
                // Extract text from image.
                OcrResult result = await ocrEngine.RecognizeAsync(bitmap);

                // Return recognized text.
                Debug.WriteLine(result.Text);
            } 
        }
        /**
        public Bitmap CropBitmap(Bitmap bitmap, int cropX, int cropY, int cropWidth, int cropHeight)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            return cropped;
        } ***/



    }
}

