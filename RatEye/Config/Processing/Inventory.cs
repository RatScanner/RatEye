using System.Drawing;

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
				}

				private void SetDefaults()
				{
					MinGridColor = Color.FromArgb(73, 81, 84);
					MaxGridColor = Color.FromArgb(112, 117, 108);
				}

				internal static void SetStaticDefaults() { }

				internal void Apply() { }
			}
		}
	}
}
