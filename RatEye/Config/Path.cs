using System;

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
			/// Base directory, which is used to create most other paths from.
			/// Defaults to the base directory of the current app domain.
			/// </summary>
			private string Base;

			/// <summary>
			/// Data directory, which is used to create some data related paths.
			/// Defaults to <code>%Base%/Data</code>
			/// </summary>
			private string Data;

			/// <summary>
			/// Path of the folder containing icons
			/// </summary>
			public string StaticIcon;

			/// <summary>
			/// Path of the folder containing dynamic icons
			/// </summary>
			public string DynamicIcon;

			/// <summary>
			/// Path of the file containing correlation data for icons and uid's
			/// </summary>
			public string StaticCorrelation;

			/// <summary>
			/// Path of the file containing correlation data for icons and uid's
			/// </summary>
			public string DynamicCorrelation;

			/// <summary>
			/// Path of the icon to return when no icon matches the query
			/// </summary>
			public string UnknownIcon;

			/// <summary>
			/// Path to the folder containing the trained LSTM model of the "bender" font.
			/// </summary>
			/// <remarks>File has to be named <c>"bender.traineddata"</c>.</remarks>
			public string BenderTraineddata;

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
			/// Base the state on the default config rather then <see cref="Config.GlobalConfig"/>
			/// </param>
			public Path(bool basedOnDefault = false)
			{
				if (basedOnDefault)
				{
					SetDefaults();
					return;
				}

				var globalConfig = GlobalConfig.PathConfig;

				Base = globalConfig.Base;
				Data = globalConfig.Data;
				StaticIcon = globalConfig.StaticIcon;
				DynamicIcon = globalConfig.DynamicIcon;
				StaticCorrelation = globalConfig.StaticCorrelation;
				DynamicCorrelation = globalConfig.DynamicCorrelation;
				UnknownIcon = globalConfig.UnknownIcon;
				BenderTraineddata = globalConfig.BenderTraineddata;
				Debug = globalConfig.Debug;
				LogFile = globalConfig.LogFile;
			}

			private void SetDefaults()
			{
				Base = AppDomain.CurrentDomain.BaseDirectory;
				Data = Combine(Base, "Data");
				StaticIcon = Combine(Data, "name");
				DynamicIcon = Combine(GetEfTTempPath(), "Icon Cache");
				StaticCorrelation = Combine(Data, "correlation.json");
				DynamicCorrelation = Combine(DynamicIcon, "index.json");
				UnknownIcon = Combine(Data, "unknown.png");
				BenderTraineddata = Data;
				Debug = Combine(Base, "Debug");
				LogFile = Combine(Base, "Log.txt");
			}

			/// <summary>
			/// Combine two paths
			/// </summary>
			/// <param name="basePath">Base path</param>
			/// <param name="x">Path to be added</param>
			/// <returns>The combined path</returns>
			private string Combine(string basePath, string x)
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
		}
	}
}
