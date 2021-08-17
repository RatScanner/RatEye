using System;

namespace RatEye
{
	public partial class Config
	{
		/// <summary>
		/// The Processing class contains parameters, which
		/// are shared amongst multiple processing types.
		/// </summary>
		public partial class Processing
		{
			/// <summary>
			/// Scale of the image. 1f when the image is from a 1080p screen, 2f when 4k, ...
			/// <para><remarks>Use <see cref="Resolution2Scale"/> to compute the scale.</remarks></para>
			/// </summary>
			public float Scale;

			/// <summary>
			/// Inverse scale of the image. 1f when the image is from a 1080p screen, 0.5f when 4k, ...
			/// <code>=> 1f / <see cref="Scale"/></code>
			/// </summary>
			internal float InverseScale => 1f / Scale;

			/// <summary>
			/// The size of a single slot on 1080p resolution, measured in pixel
			/// </summary>
			public float BaseSlotSize;

			/// <summary>
			/// Slot size of a single slot in pixels, considering for scaling
			/// <code>Scale * BaseSlotSize</code>
			/// </summary>
			internal float ScaledSlotSize => Scale * BaseSlotSize;

			/// <summary>
			/// Icon configuration object
			/// </summary>
			public Icon IconConfig;

			/// <summary>
			/// Inspection configuration object
			/// </summary>
			public Inspection InspectionConfig;

			/// <summary>
			/// Inventory configuration object
			/// </summary>
			public Inventory InventoryConfig;

			/// <summary>
			/// Create a new processing config instance based on the state of <see cref="Config.GlobalConfig"/>
			/// </summary>
			/// <param name="basedOnDefault">
			/// Base the state on the default config rather then <see cref="Config.GlobalConfig"/>
			/// </param>
			public Processing(bool basedOnDefault = false)
			{
				EnsureStaticInit();

				if (basedOnDefault)
				{
					SetDefaults();
					return;
				}

				var globalConfig = GlobalConfig.ProcessingConfig;

				Scale = globalConfig.Scale;
				BaseSlotSize = globalConfig.BaseSlotSize;
				IconConfig = globalConfig.IconConfig;
				InspectionConfig = globalConfig.InspectionConfig;
				InventoryConfig = globalConfig.InventoryConfig;
			}

			private void SetDefaults()
			{
				Scale = 1;
				BaseSlotSize = 63;
				IconConfig = new Icon(true);
				InspectionConfig = new Inspection(true);
				InventoryConfig = new Inventory(true);
			}

			internal static void SetStaticDefaults()
			{
				Icon.SetStaticDefaults();
				Inspection.SetStaticDefaults();
				Inventory.SetStaticDefaults();
			}

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

			internal void Apply()
			{
				IconConfig.Apply();
				InspectionConfig.Apply();
				InventoryConfig.Apply();
			}
		}
	}
}
