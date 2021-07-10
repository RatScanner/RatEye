using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Color = System.Drawing.Color;
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

		#region Mat Extensions

		/// <summary>
		/// Alpha blend two 8UC4 matrices
		/// </summary>
		/// <param name="bottom">Bottom matrix</param>
		/// <param name="top">Top matrix</param>
		/// <returns>A blended matrix with no transparency</returns>
		internal static Mat AlphaBlend(this Mat bottom, Mat top)
		{
			var output = new Mat(bottom.Size(), MatType.CV_8UC4);

			// Extract top alpha
			var topAlpha = top.ExtractChannel(3);
			topAlpha.ConvertTo(topAlpha, MatType.CV_32FC1);

			// Extract bottom alpha
			var bottomAlpha = bottom.ExtractChannel(3);
			bottomAlpha.ConvertTo(bottomAlpha, MatType.CV_32FC1);
			// Subtract top alpha to overlay top
			bottomAlpha = bottomAlpha.Subtract(topAlpha);

			// Blend both mats
			Cv2.BlendLinear(bottom, top, bottomAlpha, topAlpha, output);

			// Set opacity to max
			var clear = new Mat(bottom.Size(), MatType.CV_8UC4).SetTo(new Scalar(1, 1, 1, 0));
			var full = new Mat(bottom.Size(), MatType.CV_8UC4).SetTo(new Scalar(0, 0, 0, 255));
			output = output.Mul(clear).Add(full);

			return output;
		}

		/// <summary>
		/// Replicates the input matrix the specified number of times in the horizontal and/or vertical direction
		/// </summary>
		/// <param name="src">Source matrix</param>
		/// <param name="nx">How many times the src is repeated along the horizontal axis</param>
		/// <param name="ny">How many times the src is repeated along the vertical axis</param>
		/// <param name="dx">Horizontal left-padding/param>
		/// <param name="dy">Vertical top-padding</param>
		/// <returns>The repeated matrix</returns>
		internal static Mat Repeat(this Mat src, int nx, int ny, int dx, int dy)
		{
			var dst = new Mat((src.Rows + dy) * ny - dy, (src.Cols + dx) * nx - dx, src.Type());
			for (var iy = 0; iy < ny; ++iy)
			{
				for (var ix = 0; ix < nx; ++ix)
				{
					var roi = new Rect((src.Cols + dx) * ix, (src.Rows + dy) * iy, src.Cols, src.Rows);
					src.CopyTo(dst[roi]);
				}
			}

			return dst;
		}

		#endregion

		#region String Extensions

		public static float NormedLevenshteinDistance(this string source, string target)
		{
			if (string.IsNullOrEmpty(source)) { return string.IsNullOrEmpty(target) ? 0 : target.Length; }

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
