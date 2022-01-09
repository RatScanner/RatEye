using System.Drawing;
using OpenCvSharp;

namespace RatEye
{
	public partial class Config
	{
		public partial class Processing
		{
			/// <summary>
			/// The Inventory class contains parameters, used by the inventory processing module.
			/// </summary>
			public class Inventory
			{
				/// <summary>
				/// Color of the grid as defined by EFT
				/// </summary>
				internal Scalar GridColor => new(84, 81, 73, 255);

				/// <summary>
				/// Alpha value of item background colors as defined by EFT
				/// </summary>
				internal int BackgroundAlpha => 77;

				/// <summary>
				/// Minimum color for thresholding the grid
				/// </summary>
				public Color MinGridColor = Color.FromArgb(73, 81, 84);

				/// <summary>
				/// Maximum color for thresholding the grid
				/// </summary>
				/// <remarks>
				/// Recommended <c>Color.FromArgb(89, 89, 89)</c> when processing
				/// highlighted items, else <c>Color.FromArgb(112, 117, 108)</c>.
				/// This is not getting set by <see cref="OptimizeHighlighted"/>.
				/// </remarks>
				public Color MaxGridColor = Color.FromArgb(104, 112, 112);

				/// <summary>
				/// If <see langword="true"/>, all processing will be optimized for highlighted items
				/// </summary>
				/// <remarks>
				/// Consider also setting <see cref="MaxGridColor"/> to a appropriate value.
				/// </remarks>
				public bool OptimizeHighlighted = false;

				/// <summary>
				/// Create a new inventory config instance
				/// </summary>
				public Inventory() { }
			}
		}
	}
}
