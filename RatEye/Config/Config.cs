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
		/// Icon manager
		/// </summary>
		internal IconManager IconManager;

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
		public static Config GlobalConfig
		{
			get
			{
				var gc = _globalConfig;
				if (!_globalConfigInitialized)
				{
					_globalConfigInitialized = true;
					_globalConfig.Apply();
				}

				return gc;
			}
		}

		private static Config _globalConfig = new Config(true);

		/// <summary>
		/// <see langword="true"/> if static fields were initialized
		/// </summary>
		private static bool _staticsInitialized = false;

		/// <summary>
		/// <see langword="true"/> if the global config is initialized
		/// </summary>
		private static bool _globalConfigInitialized = false;

		/// <summary>
		/// Create a new config instance based on the state of <see cref="Config.GlobalConfig"/>
		/// </summary>
		/// <param name="basedOnDefault">
		/// Base the state on the default config rather then <see cref="Config.GlobalConfig"/>
		/// </param>
		public Config(bool basedOnDefault = false)
		{
			EnsureStaticInit();

			if (basedOnDefault)
			{
				SetDefaults();
				return;
			}

			var globalConfig = GlobalConfig;

			LogDebug = globalConfig.LogDebug;
			PathConfig = globalConfig.PathConfig;
			ProcessingConfig = globalConfig.ProcessingConfig;
			IconManager = globalConfig.IconManager;
		}

		private void SetDefaults()
		{
			LogDebug = false;
			PathConfig = new Path(true);
			ProcessingConfig = new Processing(true);
			// We do this after the GlobalConfig is initialized
			// IconManager = new IconManager();
		}

		private static void SetStaticDefaults()
		{
			Path.SetStaticDefaults();
			Processing.SetStaticDefaults();

			RatStashDB ??= Database.FromFile(Path.ItemData);
		}

		private static void EnsureStaticInit()
		{
			if (_staticsInitialized) return;
			_staticsInitialized = true;

			SetStaticDefaults();
		}

		/// <summary>
		/// Reinitialize all objects which depend on this config
		/// </summary>
		/// <remarks>
		/// Call this after modifying the config object
		/// </remarks>
		public Config Apply()
		{
			RatStashDB = Database.FromFile(Path.ItemData);
			IconManager = new IconManager();

			Config.GlobalConfig.PathConfig.Apply();
			Config.GlobalConfig.ProcessingConfig.Apply();
			return this;
		}
	}
}
