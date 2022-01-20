using System.Drawing;
using RatEye.Processing;
using RatStash;
using Inventory = RatEye.Processing.Inventory;

namespace RatEye
{
	public class RatEyeEngine
	{
		/// <summary>
		/// The config which is used for this <see cref="RatEyeEngine"/>
		/// </summary>
		/// <remarks>Do not modify this config object</remarks>
		public Config Config { get; }

		/// <summary>
		/// Create a <see cref="RatEyeEngine"/> instance which is the basis of all processing
		/// </summary>
		/// <param name="config">The config to use for this instance</param>
		/// <remarks>Do not modify the config after passing it</remarks>
		public RatEyeEngine(Config config)
		{
			Config = config;
			var processingCfg = Config.ProcessingConfig;
			var inspectionCfg = processingCfg.InspectionConfig;
			var inventoryCfg = processingCfg.InventoryConfig;
			var iconCfg = processingCfg.IconConfig;

			// Setup RatStash database
			var itemData = Config.PathConfig.ItemData;
			var locales = Config.PathConfig.ItemLocales;
			var langCode = Config.ProcessingConfig.Language.ToBSGCode();
			var locale = System.IO.Path.Combine(locales, $"{langCode}.json");
			config.RatStashDB = RatStash.Database.FromFile(itemData, true, locale);

			Config.IconManager = new IconManager(config);
		}

		public Inspection NewInspection(Bitmap image)
		{
			return new Inspection(image, Config);
		}

		public Inventory NewInventory(Bitmap image)
		{
			return new Inventory(image, Config);
		}
	}
}
