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
				}

				private void SetDefaults() { }

				internal static void SetStaticDefaults() { }

				internal void Apply() { }
			}
		}
	}
}
