using System;

namespace RatEye
{
	public static partial class Config
	{
		/// <summary>
		/// The Path class contains all paths used by library like the output
		/// path for log files, folders containing icons for pattern matching etc.
		/// </summary>
		public static class Path
		{
			/// <summary>
			/// Base directory, which is used to create most other paths from.
			/// Defaults to the base directory of the current app domain.
			/// </summary>
			private static string Base = AppDomain.CurrentDomain.BaseDirectory;

			/// <summary>
			/// Data directory, which is used to create some data related paths.
			/// Defaults to <code>%Base%/Data</code>
			/// </summary>
			private static string Data = Combine(Base, "Data");

			/// <summary>
			/// Path of the folder containing static icons
			/// </summary>
			public static string StaticIcon = Combine(Data, "name");

			/// <summary>
			/// Path of the folder containing dynamic icons
			/// </summary>
			public static string DynamicIcon = Combine(GetEfTTempPath(), "Icon Cache");

			/// <summary>
			/// Path of the file containing correlation data for icons and uid's
			/// </summary>
			public static string StaticCorrelation = Combine(Data, "correlation.json");

			/// <summary>
			/// Path of the file containing correlation data for icons and uid's
			/// </summary>
			public static string DynamicCorrelation = Combine(DynamicIcon, "index.json");

			/// <summary>
			/// Path of the icon to return when no icon matches the query
			/// </summary>
			public static string UnknownIcon = Combine(Data, "unknown.png");

			/// <summary>
			/// Path to the folder containing the trained LSTM model of the "bender" font.
			/// File has to be named "bender.traineddata".
			/// </summary>
			public static string BenderTraineddata = Data;

			/// <summary>
			/// Path of the debug folder which is used to store debug information
			/// </summary>
			public static string Debug = Combine(Base, "Debug");

			/// <summary>
			/// Path of the log file
			/// </summary>
			public static string LogFile => Combine(Base, "Log.txt");

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

			/// <summary>
			/// Get the directory used by eft for temporary files like the icon cache
			/// </summary>
			/// <returns>The directory used by eft for temporary files</returns>
			private static string GetEfTTempPath()
			{
				var eftTempDir = "Battlestate Games\\EscapeFromTarkov\\";
				return Combine(System.IO.Path.GetTempPath(), eftTempDir);
			}
		}
	}
}
