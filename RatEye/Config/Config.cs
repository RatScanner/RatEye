using RatStash;

namespace RatEye
{
	/// <summary>
	/// This is the config which is used for all processing
	/// </summary>
	public partial class Config
	{
		/// <summary>
		/// Log debug data
		/// </summary>
		public static bool LogDebug = false;

		/// <summary>
		/// Path configuration object
		/// </summary>
		public Path PathConfig = new Path();

		/// <summary>
		/// Processing configuration object
		/// </summary>
		public Processing ProcessingConfig = new Processing();

		/// <summary>
		/// Icon manager
		/// </summary>
		/// <remarks>
		/// Initialized in <see cref="RatEyeEngine"/>
		/// </remarks>
		internal IconManager IconManager;

		/// <summary>
		/// Item database
		/// </summary>
		/// <remarks>
		/// Initialized in <see cref="RatEyeEngine"/>
		/// </remarks>
		internal Database RatStashDB;

		/// <summary>
		/// Create a new config instance
		/// </summary>
		public Config() { }
	}
}
