using SkiaSharp;

namespace News.Models.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Creates a thumbnail from an original image stream using SkiaSharp.
        /// </summary>
        /// <param name="originalImageStream">The stream of the original image uploaded by the user.</param>
        /// <param name="outputPath">The full path where the thumbnail file will be saved.</param>
        /// <param name="width">The target width for the thumbnail (height is calculated automatically).</param>
        public static void CreateThumbnail(Stream originalImageStream, string outputPath, int width)
        {
            // 1. Decode the original image from the stream
            using (var originalBitmap = SKBitmap.Decode(originalImageStream))
            {
                // 2. Calculate the new height to maintain the aspect ratio
                var newHeight = (int)Math.Round((float)width / originalBitmap.Width * originalBitmap.Height);

                // 3. Create a new empty bitmap for the thumbnail with the new dimensions
                using (var thumbnailBitmap = new SKBitmap(width, newHeight))
                {
                    // --- FIX APPLIED HERE ---
                    // 4. Scale the original image's pixels into the new thumbnail bitmap
                    // Use SKSamplingOptions with appropriate constructor instead of 'Linear'
                    var samplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
                    originalBitmap.ScalePixels(thumbnailBitmap, samplingOptions);

                    // 5. Convert the thumbnail bitmap into a savable image format (JPEG)
                    using (var image = SKImage.FromBitmap(thumbnailBitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 75))
                    {
                        // 6. Save the final file to the specified path
                        using (var stream = File.OpenWrite(outputPath))
                        {
                            data.SaveTo(stream);
                        }
                    }
                }
            }
        }
    }
}
