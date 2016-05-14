using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.PointOfService;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace ReceiptVault
{
    /**
     * This class'll read an image using OCR, and saves it in its field "scannedText"
     */
    class ImageScan
    {
        private String scannedText;
        private int[,] position;

        private WriteableBitmap bitmap;

        public ImageScan(int[,] position)
        {
            this.position = position;
            Debug.WriteLine("position size: " + position.Length);
           // engine = new OcrEngine()

            foreach (var lang in Windows.Media.Ocr.OcrEngine.AvailableRecognizerLanguages)
            {
                Debug.WriteLine(lang.DisplayName.ToString());
            }

        }

        public async void recognize(int[,] position)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".bmp");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            StorageFile file = await picker.PickSingleFileAsync();
            ImageProperties imgProp = await file.Properties.GetImagePropertiesAsync();

            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                bitmap = new WriteableBitmap((int) imgProp.Width, (int)imgProp.Height);
                bitmap.SetSource(stream);
               // ImagePreview.Source = bitmap;
            }

            OcrEngine ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();

            //"elke tutorial: 'OcrResult result = await ocrEngine.RecognizeAsync(height, width, pixels);'... helaas.

          //  OcrResult ocrResult = await ocrEngine.RecognizeAsync();

          //  Debug.WriteLine(ocrResult.Text);

        }
    }
}
