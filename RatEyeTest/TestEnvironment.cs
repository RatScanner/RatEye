using System.Drawing;
using System.IO;
using RatEye;

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
					StaticIcons = "Data/StaticIcons",
					StaticCorrelationData = "Data/StaticIcons/correlation.json",
				},
				ProcessingConfig = new Config.Processing()
				{
					IconConfig = new Config.Processing.Icon()
					{
						UseStaticIcons = true,
						UseDynamicIcons = false,
					},
					InventoryConfig = new Config.Processing.Inventory()
					{
						OptimizeHighlighted = optimizeHighlighted,
						MaxGridColor = optimizeHighlighted ? Color.FromArgb(89, 89, 89) : Color.FromArgb(112, 117, 108),
					}
				},
			};
			return new RatEyeEngine(config);
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
