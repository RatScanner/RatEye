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
				public Color MinGridColor;

				/// <summary>
				/// Maximum color for thresholding the grid
				/// </summary>
				/// <remarks>
				/// Recommended <c>Color.FromArgb(89, 89, 89)</c> when processing
				/// highlighted items, else <c>Color.FromArgb(112, 117, 108)</c>.
				/// </remarks>
				public Color MaxGridColor;

				/// <summary>
				/// If <see langword="true"/>, all processing will be optimized for highlighted items
				/// </summary>
				public bool OptimizeHighlighted;

				/// <summary>
				/// Create a new inventory config instance based on the state of <see cref="Config.GlobalConfig"/>
				/// </summary>
				/// <param name="basedOnDefault">
				/// Base the state on the default config rather then <see cref="Config.GlobalConfig"/>
				/// </param>
				public Inventory(bool basedOnDefault = false)
				{
					EnsureStaticInit();

					if (basedOnDefault)
					{
						SetDefaults();
						return;
					}

					var globalConfig = GlobalConfig.ProcessingConfig.InventoryConfig;
					MinGridColor = globalConfig.MinGridColor;
					MaxGridColor = globalConfig.MaxGridColor;
					OptimizeHighlighted = globalConfig.OptimizeHighlighted;
				}

				private void SetDefaults()
				{
					MinGridColor = Color.FromArgb(73, 81, 84);
					MaxGridColor = Color.FromArgb(112, 117, 108);
					OptimizeHighlighted = false;
				}

				internal static void SetStaticDefaults() { }

				internal void Apply() { }
			}
		}
	}
}
