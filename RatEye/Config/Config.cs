using RatStash;

namespace RatEye
{
	public partial class Config
	{
		/// <summary>
		/// Log debug data
		/// </summary>
		public bool LogDebug;

		/// <summary>
		/// Path configuration object
		/// </summary>
		public Path PathConfig;

		/// <summary>
		/// Processing configuration object
		/// </summary>
		public Processing ProcessingConfig;

		/// <summary>
		/// Item database
		/// </summary>
		/// <remarks>
		/// Initialized in property setter of <see cref="Path.ItemData"/>
		/// </remarks>
		internal static Database RatStashDB;

		/// <summary>
		/// The global config which will be used when no override config is provided
		/// </summary>
		/// <remarks>
		/// New config objects will be based on the state of this
		/// global config instance, unless otherwise specified.
		/// </remarks>
		public static Config GlobalConfig = new Config(true);

		/// <summary>
		/// Create a new config instance based on the state of <see cref="Config.GlobalConfig"/>
		/// </summary>
		/// <param name="basedOnDefault">
		/// Base the state on the default config rather then <see cref="Config.GlobalConfig"/>
		/// </param>
		public Config(bool basedOnDefault = false)
		{
			if (basedOnDefault)
			{
				SetDefaults();
				return;
			}

			var globalConfig = GlobalConfig;

			LogDebug = globalConfig.LogDebug;
			PathConfig = globalConfig.PathConfig;
			ProcessingConfig = globalConfig.ProcessingConfig;
		}

		private void SetDefaults()
		{
			LogDebug = false;
			PathConfig = new Path(true);
			ProcessingConfig = new Processing(true);
		}
	}
}
