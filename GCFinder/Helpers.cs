using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

#pragma warning disable CA1416

namespace GCFinder;

//Let us be thankful to our lord and savior StackOverflow for most of these methods.
public static class Helpers
{
	public static Image Base64ToImage(string base64)
	{
		byte[] bytes = Convert.FromBase64String(base64);

		Image image;
		using (MemoryStream ms = new MemoryStream(bytes))
		{
			image = Image.FromStream(ms);
		}

		return image;
	}

	public static unsafe byte[] ImageToByteArray(Image imageIn)
	{
		Bitmap bmp = new(imageIn);
		BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
		int bytesPerPixel = 3;
		int heightInPixels = bitmapData.Height;
		int widthInBytes = bitmapData.Width * bytesPerPixel;
		byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

		byte[] output = new byte[3 * bmp.Width * bmp.Height];
		int i = 0;

		for (int y = 0; y < heightInPixels; y++)
		{
			byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
			for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
			{

				// calculate new pixel value
				output[i + 2] = currentLine[x];
				output[i + 1] = currentLine[x + 1];
				output[i] = currentLine[x + 2];

				i += 3;
			}
		}
		bmp.UnlockBits(bitmapData);
		return output;
	}

	public static unsafe Image ByteArrayToImage(byte[] bytesArr, int w, int h)
	{
		Bitmap bmp = new(w, h);
		BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
		int bytesPerPixel = 3;
		int heightInPixels = bitmapData.Height;
		int widthInBytes = bitmapData.Width * bytesPerPixel;
		byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

		int i = 0;

		for (int y = 0; y < heightInPixels; y++)
		{
			byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
			for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
			{

				// calculate new pixel value
				currentLine[x] = bytesArr[i + 2];
				currentLine[x + 1] = bytesArr[i + 1];
				currentLine[x + 2] = bytesArr[i];

				i += 3;
			}
		}
		bmp.UnlockBits(bitmapData);
		return bmp;
	}

	public static unsafe Image[] ByteArrayBlockToImages(byte[] bytesArr, int w, int h, int count)
	{
		Image[] ret = new Image[count];
		Parallel.For(0, count, f =>
		{
			int offset = 3 * f * w * h;
			Bitmap bmp = new(w, h);
			BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			int bytesPerPixel = 3;
			int heightInPixels = bitmapData.Height;
			int widthInBytes = bitmapData.Width * bytesPerPixel;
			byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

			int i = 0;

			for (int y = 0; y < heightInPixels; y++)
			{
				byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
				for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
				{

					// calculate new pixel value
					currentLine[x] = bytesArr[offset + i + 2];
					currentLine[x + 1] = bytesArr[offset + i + 1];
					currentLine[x + 2] = bytesArr[offset + i];

					i += 3;
				}
			}
			bmp.UnlockBits(bitmapData);
			ret[f] = bmp;
		});
		return ret;
	}

	public static unsafe Image BytePtrToImage(byte* bytePtr, int w, int h)
	{
		Bitmap bmp = new(w, h);
		BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
		int bytesPerPixel = 3;
		int heightInPixels = bitmapData.Height;
		int widthInBytes = bitmapData.Width * bytesPerPixel;
		byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

		int i = 0;

		for (int y = 0; y < heightInPixels; y++)
		{
			byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
			for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
			{

				// calculate new pixel value
				currentLine[x] = bytePtr[i + 2];
				currentLine[x + 1] = bytePtr[i + 1];
				currentLine[x + 2] = bytePtr[i];

				i += 3;
			}
		}
		bmp.UnlockBits(bitmapData);
		return bmp;
	}

	public static unsafe Image[] BytePtrToImages(byte* bytePtr, int w, int h, int count)
	{
		Image[] ret = new Image[count];
		Parallel.For(0, count, f =>
		{
			int offset = 3 * f * w * h;
			Bitmap bmp = new(w, h);
			BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			int bytesPerPixel = 3;
			int heightInPixels = bitmapData.Height;
			int widthInBytes = bitmapData.Width * bytesPerPixel;
			byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

			int i = 0;

			for (int y = 0; y < heightInPixels; y++)
			{
				byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
				for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
				{

					// calculate new pixel value
					currentLine[x] = bytePtr[offset + i + 2];
					currentLine[x + 1] = bytePtr[offset + i + 1];
					currentLine[x + 2] = bytePtr[offset + i];

					i += 3;
				}
			}
			bmp.UnlockBits(bitmapData);
			ret[f] = bmp;
		});
		return ret;
	}
}
