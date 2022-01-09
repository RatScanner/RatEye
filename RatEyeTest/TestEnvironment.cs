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

		public RatEyeEngine GetDefaultRatEyeEngine()
		{
			var config = new Config();
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
