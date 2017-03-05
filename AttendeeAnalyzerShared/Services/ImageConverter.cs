using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace AttendeeAnalyzerUWPShared.Services
{
    public static class ImageConverter
    {
        public static async Task<SoftwareBitmap> ConvertToSoftwareBitmapAsync(InMemoryRandomAccessStream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);

            var transform = new BitmapTransform();
            const float sourceImageHeightLimit = 1280;

            if (decoder.PixelHeight > sourceImageHeightLimit)
            {
                var scalingFactor = (float)sourceImageHeightLimit / (float)decoder.PixelHeight;
                transform.ScaledWidth = (uint)Math.Floor(decoder.PixelWidth * scalingFactor);
                transform.ScaledHeight = (uint)Math.Floor(decoder.PixelHeight * scalingFactor);
            }

            var sourceBitmap = await decoder.GetSoftwareBitmapAsync(decoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

            const BitmapPixelFormat faceDetectionPixelFormat = BitmapPixelFormat.Gray8;

            SoftwareBitmap convertedBitmap;

            if (sourceBitmap.BitmapPixelFormat != faceDetectionPixelFormat)
            {
                convertedBitmap = SoftwareBitmap.Convert(sourceBitmap, faceDetectionPixelFormat);
            }
            else
            {
                convertedBitmap = sourceBitmap;
            }

            return convertedBitmap;
        }

        public static Stream ConvertImage(IRandomAccessStream inputStream)
        {
            var ms = new MemoryStream();
            var stream = inputStream.AsStream();
            stream.Position = 0;
            stream.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
