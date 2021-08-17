using System.IO;

namespace RatEyeTest
{
	public class TestEnvironment
	{
		public TestEnvironment()
		{
			RatEye.Config.GlobalConfig.LogDebug = true;
			RatEye.Config.GlobalConfig.Apply();
			Setup();
		}

		protected virtual void Setup()
		{
			if (Directory.Exists("Debug")) Directory.Delete("Debug", true);
			Directory.CreateDirectory("Debug");
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
