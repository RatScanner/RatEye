using System;

namespace RatEyeTest
{
	public class TestEnvironment
	{
		public TestEnvironment()
		{
			RatEye.Config.GlobalConfig.LogDebug = true;
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
