using System.IO;
using RatEye;
using RatStash;
using Color = System.Drawing.Color;

namespace RatEyeTest
{
	public class TestEnvironment
	{
		private static bool _initialized;

		public TestEnvironment()
		{
			RatEye.Config.LogDebug = true;
			var debugFolder = RatEye.Config.Path.Debug;

			if (!_initialized)
			{
				_initialized = true;
				if (Directory.Exists(debugFolder)) Directory.Delete(debugFolder, true);
				Directory.CreateDirectory(debugFolder);
			}
		}

		public RatEyeEngine GetDefaultRatEyeEngine(bool optimizeHighlighted = false)
		{
			var config = new Config()
			{
				PathConfig = new Config.Path()
				{
					StaticIcons = "Data/Icons",
					StaticCorrelationData = "Data/Icons/correlation.json",
				},
				ProcessingConfig = new Config.Processing()
				{
					IconConfig = new Config.Processing.Icon()
					{
						UseStaticIcons = true,
					},
					InventoryConfig = new Config.Processing.Inventory()
					{
						OptimizeHighlighted = optimizeHighlighted,
					}
				},
			};
			
			return new RatEyeEngine(config, GetItemDatabase());
		}

		public static RatStash.Database GetItemDatabase(RatStash.Language language = Language.English, bool filtered = true)
		{
			var localePath = $"Data/locales/{language.ToBSGCode()}.json";
			var itemDatabase =  RatStash.Database.FromFile("Data/items.json", filtered, localePath);
			return itemDatabase.Filter(item => !item.QuestItem
												&& item.GetType() != typeof(LootContainer)
												&& item.GetType() != typeof(Pockets));
		}

		/// <summary>
		/// Combine two paths
		/// </summary>
		/// <param name="basePath">Base path</param>
		/// <param name="x">Path to be added</param>
		/// <returns>The combined path</returns>
		private static string Combine(string basePath, string x)
		{
			return System.IO.Path.Combine(basePath, x);
		}
	}
}
