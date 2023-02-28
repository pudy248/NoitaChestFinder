using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace GCFinder;

public static class Helpers
{
	public static string ToHex(Rgb24 color)
	{
		return (color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2")).ToLower();
	}

	public static unsafe byte[] ImageToByteArray(Image<Rgb24> image)
	{
		byte[] ret = new byte[image.Width * image.Height * 3];
		int i = 0;

		for (int y = 0; y < image.Height; y++)
		{
			for (int x = 0; x < image.Width; x++)
			{
				ret[i++] = image[x, y].R;
				ret[i++] = image[x, y].G;
				ret[i++] = image[x, y].B;
			}
		}
		return ret;
	}

	public static unsafe Image ByteArrayToImage(byte[] bytesArr, int w, int h)
	{
		Image<Rgb24> ret = new Image<Rgb24>(Configuration.Default, w, h);
		int i = 0;

		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				Rgb24 rgb = new Rgb24() { R = bytesArr[i++], G = bytesArr[i++], B = bytesArr[i++] };
				ret[x, y] = rgb;
			}
		}
		return ret;
	}

	public static unsafe Image BytePtrToImage(byte* bytePtr, int w, int h)
	{
		byte[] arr = new byte[w * h * 3];
		Marshal.Copy((IntPtr)bytePtr, arr, 0, w * h * 3);
		return ByteArrayToImage(arr, w, h);
	}
}
