using System.Drawing;
using RatEye.Processing;
using RatStash;
using Inventory = RatEye.Processing.Inventory;

namespace RatEye
{
	public class RatEyeEngine
	{
		private Config _config;

		/// <summary>
		/// Create a <see cref="RatEyeEngine"/> instance which is the basis of all processing
		/// </summary>
		/// <param name="config">The config to use for this instance</param>
		/// <remarks>Do not modify the config after passing it</remarks>
		public RatEyeEngine(Config config)
		{
			_config = config;
			var processingCfg = _config.ProcessingConfig;
			var inspectionCfg = processingCfg.InspectionConfig;
			var inventoryCfg = processingCfg.InventoryConfig;
			var iconCfg = processingCfg.IconConfig;

			// Setup RatStash database
			var itemData = _config.PathConfig.ItemData;
			var locales = _config.PathConfig.ItemLocales;
			var langCode = _config.ProcessingConfig.Language.ToBSGCode();
			var locale = System.IO.Path.Combine(locales, $"{langCode}.json");
			config.RatStashDB = RatStash.Database.FromFile(itemData, false, locale);

			_config.IconManager = new IconManager(config);
		}

		public Inspection NewInspection(Bitmap image)
		{
			return new Inspection(image, _config);
		}

		public Inventory NewInventory(Bitmap image)
		{
			return new Inventory(image, _config);
		}
	}
}
