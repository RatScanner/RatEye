using System;
using RatStash;

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
			private static string BaseDir;

			/// <summary>
			/// DataDir directory, which is used to create some data related paths.
			/// Defaults to <code>%BaseDir%/DataDir</code>
			/// </summary>
			private static string DataDir;

			/// <summary>
			/// Path of the folder containing static icons
			/// </summary>
			public string StaticIcons;

			/// <summary>
			/// Path of the folder containing dynamic icons
			/// </summary>
			public string DynamicIcons;

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
			public string StaticCorrelationData;

			/// <summary>
			/// Path of the file containing correlation data for icons and uid's.
			/// The file must be a json file and be able to be parsed by <see cref="RatStash"/>.
			/// </summary>
			public string DynamicCorrelationData;

			/// <summary>
			/// Path of the icon to return when no icon matches the query
			/// </summary>
			public string UnknownIcon;

			/// <summary>
			/// Path to the folder containing the trained LSTM model of the "bender" font
			/// </summary>
			/// <remarks>File has to be named <c>"bender.traineddata"</c>.</remarks>
			public string BenderTraineddata;

			/// <summary>
			/// Path the the file, which will be used to create a <see cref="RatStash.Database"/> instance
			/// </summary>
			/// <remarks>
			/// For more details on <see cref="RatStash"/>, see <see href="https://github.com/RatScanner/RatStash"/>
			/// </remarks>
			public static string ItemData
			{
				get
				{
					if (_itemData == null) EnsureStaticInit();
					return _itemData;
				}
				set
				{
					RatStashDB = Database.FromFile(value);
					_itemData = value;
				}
			}

			private static string _itemData;

			/// <summary>
			/// Path of the debug folder which is used to store debug information
			/// </summary>
			public string Debug;

			/// <summary>
			/// Path of the log file
			/// </summary>
			public string LogFile;

			/// <summary>
			/// Create a new path config instance based on the state of <see cref="Config.GlobalConfig"/>
			/// </summary>
			/// <param name="basedOnDefault">
			/// BaseDir the state on the default config rather then <see cref="Config.GlobalConfig"/>
			/// </param>
			public Path(bool basedOnDefault = false)
			{
				EnsureStaticInit();

				if (basedOnDefault)
				{
					SetDefaults();
					return;
				}

				var globalConfig = GlobalConfig.PathConfig;

				StaticIcons = globalConfig.StaticIcons;
				DynamicIcons = globalConfig.DynamicIcons;
				StaticCorrelationData = globalConfig.StaticCorrelationData;
				DynamicCorrelationData = globalConfig.DynamicCorrelationData;
				UnknownIcon = globalConfig.UnknownIcon;
				BenderTraineddata = globalConfig.BenderTraineddata;
				// Do not allow overwrite of ItemData since it static
				// ItemData = globalConfig.ItemData;
				Debug = globalConfig.Debug;
				LogFile = globalConfig.LogFile;
			}

			private void SetDefaults()
			{
				StaticIcons = Combine(DataDir, "staticIcons");
				DynamicIcons = Combine(GetEfTTempPath(), "Icon Cache");
				StaticCorrelationData = Combine(StaticIcons, "correlation.json");
				DynamicCorrelationData = Combine(DynamicIcons, "index.json");
				UnknownIcon = Combine(DataDir, "unknown.png");
				BenderTraineddata = DataDir;
				Debug = Combine(BaseDir, "Debug");
				LogFile = Combine(BaseDir, "Log.txt");
			}

			internal static void SetStaticDefaults()
			{
				BaseDir = AppDomain.CurrentDomain.BaseDirectory;
				DataDir = Combine(BaseDir, "Data");
				_itemData = Combine(DataDir, "items.json");
			}

			/// <summary>
			/// Combine two paths
			/// </summary>
			/// <param name="basePath">BaseDir path</param>
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
			private string GetEfTTempPath()
			{
				var eftTempDir = "Battlestate Games\\EscapeFromTarkov\\";
				return Combine(System.IO.Path.GetTempPath(), eftTempDir);
			}

			internal void Apply() { }
		}
	}
}
