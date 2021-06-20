using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using RatStash;

namespace RatEye
{
	internal class IconManager
	{
		private enum IconType
		{
			Static,
			Dynamic,
		}

		private Config _config;

		/// <summary>
		/// Static icons are those which are rendered ahead of time.
		/// For example keys, medical supply's, containers, standalone mods,
		/// and especially items like screws, drill, wires, milk and so on.
		/// <para/>
		/// <c>Dictionary&lt;slotSize, Dictionary&lt;iconKey, icon&gt;&gt;</c>
		/// </summary>
		/// <remarks>Use the <see cref="StaticIconsLock"/> when accessing this collection.</remarks>
		internal Dictionary<Vector2, Dictionary<string, Mat>> StaticIcons = new();

		/// <summary>
		/// Dynamic icons are those which need to be rendered at runtime
		/// due to the items appearance being altered by attached items.
		/// For example weapons are considered dynamic items since their
		/// icon changes when you add rails, magazines, scopes and so on.
		/// <para/>
		/// <c>ConcurrentDictionary&lt;slotSize, Dictionary&lt;iconKey, icon&gt;&gt;</c>
		/// </summary>
		/// <remarks>Use the <see cref="DynamicIconsLock"/> when accessing this collection.</remarks>
		internal Dictionary<Vector2, Dictionary<string, Mat>> DynamicIcons = new();

		/// <summary>
		/// Reader / Writer lock of <see cref="StaticIcons"/>
		/// </summary>
		internal readonly ReaderWriterLockSlim StaticIconsLock = new();

		/// <summary>
		/// Reader / Writer lock of <see cref="DynamicIcons"/>
		/// </summary>
		internal readonly ReaderWriterLockSlim DynamicIconsLock = new();

		/// <summary>
		/// The icon paths connected to each icon key
		/// <para/> ConcurrentDictionary&lt;iconKey, iconPath&gt;
		/// </summary>
		private readonly Dictionary<string, string> _iconPaths = new();

		private FileSystemWatcher _dynCorrelationDataWatcher;

		/// <summary>
		/// The data used to match icon keys of static icons to their item
		/// <para/>
		/// <c>Dictionary&lt;iconKey, item&gt;</c>
		/// </summary>
		private Dictionary<string, Item> _staticCorrelationData = new();

		/// <summary>
		/// The data used to match icon keys of dynamic icons to their item
		/// <para/>
		/// <c>Dictionary&lt;iconKey, item&gt;</c>
		/// </summary>
		private Dictionary<string, Item> _dynamicCorrelationData = new();

		/// <summary>
		/// Reader / Writer lock of <see cref="_staticCorrelationDataLock"/>
		/// </summary>
		private readonly ReaderWriterLockSlim _staticCorrelationDataLock = new();

		/// <summary>
		/// Reader / Writer lock of <see cref="_dynamicCorrelationDataLock"/>
		/// </summary>
		private readonly ReaderWriterLockSlim _dynamicCorrelationDataLock = new();

		/// <summary>
		/// Constructor for icon manager object
		/// </summary>
		/// <param name="overrideConfig">When provided, will be used instead of <see cref="Config.GlobalConfig"/></param>
		/// <remarks>Depends on <see cref="Config.Processing.Icon"/> and <see cref="Config.Path"/></remarks>
		internal IconManager(Config overrideConfig = null)
		{
			_config = overrideConfig ?? Config.GlobalConfig;

			var iconConfig = _config.ProcessingConfig.IconConfig;
			if (iconConfig.UseStaticIcons) LoadStaticIcons();
			if (iconConfig.UseDynamicIcons) LoadDynamicIcons();
			if (iconConfig.UseDynamicIcons && iconConfig.WatchDynamicIcons) InitFileWatcher();
		}

		#region Icon loading

		private void LoadStaticIcons()
		{
			LoadStaticCorrelationData();

			var temp = LoadIcons(_config.PathConfig.StaticIcons, IconType.Static);
			StaticIconsLock.EnterWriteLock();
			try { StaticIcons = temp; }
			finally { StaticIconsLock.ExitWriteLock(); }
		}

		private void LoadDynamicIcons()
		{
			LoadDynamicCorrelationData();

			var temp = LoadIcons(_config.PathConfig.DynamicIcons, IconType.Dynamic, 2);
			DynamicIconsLock.EnterWriteLock();
			try { DynamicIcons = temp; }
			finally { DynamicIconsLock.ExitWriteLock(); }
		}

		private Dictionary<Vector2, Dictionary<string, Mat>> LoadIcons(
			string folderPath,
			IconType iconType,
			int retryCount = 0)
		{
			if (!Directory.Exists(folderPath))
			{
				var message = "Could not find icon folder at: " + folderPath;
				throw new FileNotFoundException(message);
			}

			var loadedIcons = new Dictionary<Vector2, Dictionary<string, Mat>>();
			try
			{
				var iconPathArray = Directory.GetFiles(folderPath, "*.png");

				Parallel.ForEach(iconPathArray, iconPath =>
				{
					var iconKey = GetIconKey(iconPath, iconType);
					var mat = Cv2.ImRead(iconPath, ImreadModes.Unchanged);

					// Do not add the icon to the list, if its size cannot be converted to slots
					if (!IsValidPixelSize(mat.Width) || !IsValidPixelSize(mat.Height)) return;

					var size = new Vector2(PixelsToSlots(mat.Width), PixelsToSlots(mat.Height));
					lock (loadedIcons)
					{
						if (!loadedIcons.ContainsKey(size)) { loadedIcons.Add(size, new Dictionary<string, Mat>()); }

						// Add icon to icon and path dictionary
						loadedIcons[size][iconKey] = mat;
						_iconPaths[iconKey] = iconPath;
					}
				});
			}
			catch (Exception e)
			{
				Logger.LogDebug("Could not load icons!", e);
				if (retryCount > 0)
				{
					Thread.Sleep(100);
					return LoadIcons(folderPath, iconType, retryCount - 1);
				}
			}

			return loadedIcons;
		}

		#endregion

		#region Correlation Data Loading

		private void LoadStaticCorrelationData()
		{
			var path = _config.PathConfig.StaticCorrelationData;
			var correlations = JArray.Parse(ReadFileNonBlocking(path));

			var correlationData = new Dictionary<string, Item>();
			foreach (var jToken in correlations)
			{
				var correlation = (JObject)jToken;
				var iconPath = correlation.GetValue("icon").ToString();
				var uid = correlation.GetValue("uid").ToString();

				var iconKey = GetIconKey(iconPath, IconType.Static);
				correlationData[iconKey] = Config.RatStashDB.GetItem(uid);
			}

			_staticCorrelationDataLock.EnterWriteLock();
			try { _staticCorrelationData = correlationData; }
			finally { _staticCorrelationDataLock.ExitWriteLock(); }
		}

		private void LoadDynamicCorrelationData()
		{
			var path = _config.PathConfig.DynamicCorrelationData;
			var parsedIndex = Config.RatStashDB.ParseItemCacheIndex(path);
			var correlationData = parsedIndex.ToDictionary(
				x => GetIconKey(x.Key + ".png", IconType.Dynamic), x => x.Value.item);

			_dynamicCorrelationDataLock.EnterWriteLock();
			try { _dynamicCorrelationData = correlationData; }
			finally { _dynamicCorrelationDataLock.ExitWriteLock(); }
		}

		#endregion

		/// <summary>
		/// Initialize a file watcher for the dynamic correlation data
		/// to update the dynamic icons when something changes
		/// </summary>
		private void InitFileWatcher()
		{
			Logger.LogDebug("Initializing file watcher for dynamic correlation data...");
			_dynCorrelationDataWatcher = new FileSystemWatcher();
			_dynCorrelationDataWatcher.Path = Path.GetDirectoryName(_config.PathConfig.DynamicIcons);
			_dynCorrelationDataWatcher.Filter = Path.GetFileName(_config.PathConfig.DynamicCorrelationData);
			_dynCorrelationDataWatcher.NotifyFilter = NotifyFilters.Size;
			_dynCorrelationDataWatcher.Changed += OnDynamicCorrelationDataChange;
			_dynCorrelationDataWatcher.EnableRaisingEvents = true;
		}

		/// <summary>
		/// Event which gets called when the dynamic correlation data file
		/// get changed. Dynamic icons and correlation data get updated.
		/// </summary>
		private void OnDynamicCorrelationDataChange(object source, FileSystemEventArgs e)
		{
			Logger.LogDebug("Dynamic correlation data changed");

			LoadDynamicCorrelationData();
			LoadDynamicIcons();
		}

		/// <summary>
		/// Get the unique icon key for a icon path and its type
		/// </summary>
		/// <param name="iconPath">The path to the icon</param>
		/// <param name="iconType">The type of the icon</param>
		/// <returns></returns>
		private static string GetIconKey(string iconPath, IconType iconType)
		{
			var iconFile = Path.GetFileName(iconPath);
			return iconFile + iconType;
		}

		/// <summary>
		/// Get the item, referenced by its icon key
		/// </summary>
		/// <param name="iconKey"></param>
		/// <returns>The matching item</returns>
		internal Item GetItem(string iconKey)
		{
			if (iconKey.EndsWith(IconType.Static.ToString()))
			{
				_staticCorrelationDataLock.EnterReadLock();
				try { return _staticCorrelationData[iconKey]; }
				finally { _staticCorrelationDataLock.ExitReadLock(); }
			}

			if (iconKey.EndsWith(IconType.Dynamic.ToString()))
			{
				_dynamicCorrelationDataLock.EnterReadLock();
				try { return _dynamicCorrelationData[iconKey]; }
				finally { _dynamicCorrelationDataLock.ExitReadLock(); }
			}

			return null;
		}

		/// <summary>
		/// Converts the pixel unit of a icon into the slot unit
		/// </summary>
		/// <param name="pixels">The pixel size of the icon</param>
		/// <returns>Slot size of the icon</returns>
		private int PixelsToSlots(int pixels)
		{
			// Use converter class to round to nearest int instead of always rounding down
			return Convert.ToInt32((pixels - 1) / _config.ProcessingConfig.BaseSlotSize);
		}

		/// <summary>
		/// Checks if the give pixels can be converted into slot unit
		/// </summary>
		/// <param name="pixels">The pixel size of the icon</param>
		/// <returns>True if the pixels can be converted to slots</returns>
		private bool IsValidPixelSize(int pixels)
		{
			return Math.Abs(1 - pixels % _config.ProcessingConfig.BaseSlotSize) < 0.01f;
		}

		/// <summary>
		/// Reads a file with <see cref="FileShare.ReadWrite"/>
		/// </summary>
		/// <param name="path">The path of the file</param>
		/// <returns>The file content as string</returns>
		private static string ReadFileNonBlocking(string path)
		{
			using var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var textReader = new StreamReader(fileStream);
			return textReader.ReadToEnd();
		}

		~IconManager()
		{
			_dynCorrelationDataWatcher?.Dispose();
		}
	}
}
