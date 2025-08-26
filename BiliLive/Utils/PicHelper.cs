using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace BiliLive.Utils;

public static class PicHelper
{
    public static Bitmap? ResizeStreamToBitmap(Stream ms, int targetWidth, int targetHeight)
    {
        using var original = SKBitmap.Decode(ms);
        if (original == null)
            return null;

        targetWidth *= 2;
        targetHeight *= 2;
        
        // 目标比例
        float targetRatio = (float)targetWidth / targetHeight;
        float originalRatio = (float)original.Width / original.Height;

        SKRectI cropRect;

        if (originalRatio > targetRatio)
        {
            // 原图比目标宽 → 裁掉左右
            int cropWidth = (int)(original.Height * targetRatio);
            int cropX = (original.Width - cropWidth) / 2;
            cropRect = new SKRectI(cropX, 0, cropX + cropWidth, original.Height);
        }
        else
        {
            // 原图比目标高 → 裁掉上下
            int cropHeight = (int)(original.Width / targetRatio);
            int cropY = (original.Height - cropHeight) / 2;
            cropRect = new SKRectI(0, cropY, original.Width, cropY + cropHeight);
        }

        using var cropped = new SKBitmap(cropRect.Width, cropRect.Height);
        original.ExtractSubset(cropped, cropRect);

        // 缩放到目标大小
        var info = new SKImageInfo(targetWidth, targetHeight);
        using var resized = new SKBitmap(info);
        cropped.ScalePixels(resized, SKFilterQuality.High);

        // 转成 Avalonia Bitmap
        using var image = SKImage.FromBitmap(resized);
        var outputStream = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(outputStream);
        outputStream.Position = 0;
        
        // Debug: 保存原图和处理后的图
        // var originalBytes = ms is MemoryStream msMem ? msMem.ToArray() : null;
        // if (originalBytes != null)
        // {
        //     File.WriteAllBytes("original.png", originalBytes);
        // }
        //
        // var outBytes = outputStream.ToArray();
        // File.WriteAllBytes("resized_image.png", outBytes);
        
        return new Bitmap(outputStream);
    }
}