using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = System.Drawing.Point;
using Size = OpenCvSharp.Size;

namespace RatEye
{
	public static class Extensions
	{
		#region Bitmap Extensions

		/// <summary>
		/// Rescales a bitmap
		/// </summary>
		/// <param name="image">The image to be rescaled</param>
		/// <param name="scale">Scale factor by which the dimensions will be multiplied</param>
		/// <returns>The rescaled input image</returns>
		/// <remarks>
		/// Uses <see cref="InterpolationFlags.Area"/> for downscaling and <see cref="InterpolationFlags.Cubic"/> for upscaling
		/// </remarks>
		public static Bitmap Rescale(this Bitmap image, float scale)
		{
			if (scale == 1f) return image;

			var mat = image.ToMat();
			var rescaledSize = new Size((int)(mat.Width * scale), (int)(mat.Height * scale));
			var rescaleMode = scale < 1 ? InterpolationFlags.Area : InterpolationFlags.Cubic;
			var rescaledMat = mat.Resize(rescaledSize, 0, 0, rescaleMode);
			var rescaledImage = rescaledMat.ToBitmap();
			return rescaledImage;
		}


		/// <summary>
		/// Rotates a bitmap
		/// </summary>
		/// <param name="image">The image to be rotated</param>
		/// <returns>The rotated input image</returns>
		public static Bitmap Rotate(this Bitmap image)
		{
			var mat = image.ToMat();
			var rotatedMat = new Mat();
			Cv2.Rotate(mat, rotatedMat, RotateFlags.Rotate90Counterclockwise);
			return rotatedMat.ToBitmap();
		}

		/// <summary>
		/// Alpha blends the input image with a target color
		/// </summary>
		/// <param name="image">The input image</param>
		/// <param name="target">The color which will replace the transparency</param>
		/// <returns>The input image with replaced alpha in 24bppRgb format</returns>
		public static Bitmap TransparentToColor(this Bitmap image, Color target)
		{
			var output = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
			var rect = new Rectangle(Point.Empty, image.Size);
			using (var g = Graphics.FromImage(output))
			{
				g.Clear(target);
				g.DrawImageUnscaledAndClipped(image, rect);
			}

			return output;
		}

		/// <summary>
		/// Crops the input image
		/// </summary>
		/// <param name="image">The input image</param>
		/// <param name="x">Horizontal position of the lower left corner</param>
		/// <param name="y">Vertical position of the lower left corner</param>
		/// <param name="width">Width of the output image</param>
		/// <param name="height">Height of the output image</param>
		/// <returns>The cropped input image</returns>
		/// <exception cref="ArgumentException">x, y, width and height accept non negative values only</exception>
		/// <exception cref="ArgumentOutOfRangeException">The input image is not big enough for the desired crop</exception>
		public static Bitmap Crop(this Bitmap image, int x, int y, int width, int height)
		{
			if (x < 0 || y < 0 || width < 0 || height < 0)
			{
				const string message = "x, y, width and height accept non negative values only.";
				throw new ArgumentException(message);
			}

			if (x + width > image.Width || y + height > image.Height)
			{
				const string message = "The input image is not big enough for the desired crop";
				throw new ArgumentOutOfRangeException(nameof(image), message);
			}

			var rect = new Rectangle(x, y, width, height);
			return new Bitmap(image).Clone(rect, image.PixelFormat);
		}

		/// <summary>
		/// Finds the horizontal position of the first pixel, on a given height and in a color range.
		/// Pixels are iterated from left to right.
		/// </summary>
		/// <param name="image">The input image</param>
		/// <param name="searchHeight">The height on which to search for a matching pixel</param>
		/// <param name="lowerBoundColor">The lower bound matching color</param>
		/// <param name="upperBoundColor">The upper bound matching color</param>
		/// <param name="start">Start position for searching</param>
		/// <returns>The horizontal position of the first matching pixel. -1 if no match was found</returns>
		internal static int FindPixelInRange(
			this Bitmap image,
			int searchHeight,
			Color lowerBoundColor,
			Color upperBoundColor,
			int start = 0)
		{
			var mat = image.ToMat();
			var mat3 = new Mat<Vec3b>(mat);
			var indexer = mat3.GetIndexer();

			for (var x = start; x < mat.Width; x++)
			{
				var (blue, green, red) = indexer[searchHeight, x];

				if (blue < lowerBoundColor.B || blue > upperBoundColor.B) continue;
				if (green < lowerBoundColor.G || green > upperBoundColor.G) continue;
				if (red < lowerBoundColor.R || red > upperBoundColor.R) continue;

				return x;
			}

			return -1;
		}

		#endregion

		#region String Extensions

		public static float NormedLevenshteinDistance(this string source, string target)
		{
			if (string.IsNullOrEmpty(source))
			{
				return string.IsNullOrEmpty(target) ? 0 : target.Length;
			}

			if (string.IsNullOrEmpty(target)) return source.Length;

			if (source.Length > target.Length)
			{
				var temp = target;
				target = source;
				source = temp;
			}

			var m = target.Length;
			var n = source.Length;
			var distance = new int[2, m + 1];
			// Initialize the distance matrix
			for (var j = 1; j <= m; j++) distance[0, j] = j;

			var currentRow = 0;
			for (var i = 1; i <= n; ++i)
			{
				currentRow = i & 1;
				distance[currentRow, 0] = i;
				var previousRow = currentRow ^ 1;
				for (var j = 1; j <= m; j++)
				{
					var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
					distance[currentRow, j] = Math.Min(Math.Min(
							distance[previousRow, j] + 1,
							distance[currentRow, j - 1] + 1),
						distance[previousRow, j - 1] + cost);
				}
			}

			return (float)(target.Length - distance[currentRow, m]) / target.Length;
		}

		#endregion
	}
}
