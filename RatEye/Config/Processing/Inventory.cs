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
				public (int blue, int green, int red) MinGridColor;

				/// <summary>
				/// Maximum color for thresholding the grid
				/// </summary>
				/// <remarks>
				/// Recommended <c>(89, 89, 89)</c> when processing
				/// highlighted items, else <c>(108, 117, 112)</c>.
				/// </remarks>
				public (int blue, int green, int red) MaxGridColor;

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
					MinGridColor = (84, 81, 73);
					MaxGridColor = (108, 117, 112);
				}

				internal static void SetStaticDefaults() { }

				internal void Apply() { }
			}
		}
	}
}
