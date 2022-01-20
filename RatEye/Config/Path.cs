using System;
using Tesseract;

namespace RatEye
{
	public partial class Config
	{
		/// <summary>
		/// The Path class contains all paths used by library like the output
		/// path for log files, folders containing icons for pattern matching etc.
		/// </summary>
		public class Path
		{
			/// <summary>
			/// BaseDir directory, which is used to create most other paths from.
			/// Defaults to the base directory of the current app domain.
			/// </summary>
			private static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

			/// <summary>
			/// DataDir directory, which is used to create some data related paths.
			/// Defaults to <code>%BaseDir%/DataDir</code>
			/// </summary>
			private static string DataDir => Combine(BaseDir, "Data");

			/// <summary>
			/// Path of the folder containing static icons
			/// </summary>
			public string StaticIcons = Combine(DataDir, "name");

			/// <summary>
			/// Path of the folder containing dynamic icons
			/// </summary>
			public string DynamicIcons = Combine(GetEfTTempPath(), "Icon Cache");

			/// <summary>
			/// Path of the file containing correlation data for icons and uid's.
			/// The file must be a json file with the following content:
			/// <code>
			/// [
			///		{
			///			"icon": "item_ram_module.png",
			///			"uid": "57347baf24597738002c6178"
			///		}, ...
			/// ]
			/// </code>
			/// </summary>
			public string StaticCorrelationData = Combine(DataDir, "correlation.json");

			/// <summary>
			/// Path of the file containing correlation data for icons and uid's.
			/// The file must be a json file and be able to be parsed by <see cref="RatStash"/>.
			/// </summary>
			public string DynamicCorrelationData = Combine(GetEfTTempPath(), "Icon Cache", "index.json");

			/// <summary>
			/// Path of the icon to return when no icon matches the query
			/// </summary>
			public string UnknownIcon = Combine(DataDir, "unknown.png");

			/// <summary>
			/// Path to the folder containing LSTM model files
			/// </summary>
			/// <remarks>Files have to be of the format <c>[ISO-639-3].traineddata</c>.</remarks>
			public string TrainedData = Combine(DataDir, "traineddata");

			/// <summary>
			/// Path to the file, which will be used to create the <see cref="RatStash.Database"/> instance
			/// </summary>
			/// <remarks>
			/// For more details on <see cref="RatStash"/>, see <see href="https://github.com/RatScanner/RatStash"/>
			/// </remarks>
			public string ItemData = Combine(DataDir, "items.json");

			/// <summary>
			/// Path the folder, which contains localization files used to create the <see cref="RatStash.Database"/> instance
			/// </summary>
			/// <remarks>
			/// For more details on <see cref="RatStash"/>, see <see href="https://github.com/RatScanner/RatStash"/>
			/// </remarks>
			public string ItemLocales = Combine(DataDir, "locales");

			/// <summary>
			/// Path at which tesseract searches for dependencies
			/// </summary>
			public static string TesseractLibSearchPath
			{
				get => TesseractEnviornment.CustomSearchPath;
				set => TesseractEnviornment.CustomSearchPath = value;
			}

			/// <summary>
			/// Path of the debug folder which is used to store debug information
			/// </summary>
			public static string Debug = Combine(BaseDir, "Debug");

			/// <summary>
			/// Path of the log file
			/// </summary>
			public static string LogFile = Combine(BaseDir, "Log.txt");

			/// <summary>
			/// Create a new path config instance
			/// </summary>
			public Path() { }

			/// <summary>
			/// Combine two paths
			/// </summary>
			/// <param name="basePath">BaseDir path</param>
			/// <param name="a">Path to be added</param>
			/// <param name="b">Path to be added</param>
			/// <param name="c">Path to be added</param>
			/// <returns>The combined path</returns>
			private static string Combine(string basePath, string a, string b = "", string c = "")
			{
				return System.IO.Path.Combine(basePath, a, b, c);
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
