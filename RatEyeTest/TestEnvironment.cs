﻿using System;

namespace RatEyeTest
{
	public class TestEnvironment
	{
		public TestEnvironment()
		{
			var basePath = AppDomain.CurrentDomain.BaseDirectory;
			RatEye.Config.LogDebug = true;
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