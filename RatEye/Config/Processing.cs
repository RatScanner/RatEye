using System;

namespace RatEye
{
	public static partial class Config
	{
		/// <summary>
		/// The Processing class contains parameters, which
		/// are shared amongst multiple processing types.
		/// </summary>
		public static partial class Processing
		{
			/// <summary>
			/// Scale of the image. 1f when the image is from a 1080p screen, 2f when 4k, ...
			/// <para><remarks>Use <see cref="Resolution2Scale"/> to compute the scale.</remarks></para>
			/// </summary>
			public static float Scale = 1f;

			/// <summary>
			/// Inverse scale of the image. 1f when the image is from a 1080p screen, 0.5f when 4k, ...
			/// <code>=> 1f / <see cref="Scale"/></code>
			/// </summary>
			internal static float InverseScale => 1f / Scale;

			/// <summary>
			/// The size of a single slot on 1080p resolution, measured in pixel
			/// </summary>
			public static float BaseSlotSize = 63f;

			/// <summary>
			/// Slot size of a single slot in pixels, considering for scaling
			/// <code>Scale * BaseSlotSize</code>
			/// </summary>
			internal static float ScaledSlotSize => Scale * BaseSlotSize;

			/// <summary>
			/// Convert a screen resolution to the corresponding scale
			/// </summary>
			/// <param name="width"></param>
			/// <param name="height"></param>
			/// <returns>The scale calculated from the screen resolution</returns>
			public static float Resolution2Scale(int width, int height)
			{
				var screenScaleFactor1 = width / 1920f;
				var screenScaleFactor2 = height / 1080f;

				return Math.Min(screenScaleFactor1, screenScaleFactor2);
			}
		}
	}
}
