using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using Size = System.Drawing.Size;

namespace RatEye
{
	// TODO make non static and add possibility of override config
	public static class IconManager
	{
		private static Config.Path PathConfig => Config.GlobalConfig.PathConfig;
		private static Config.Processing ProcessingConfig => Config.GlobalConfig.ProcessingConfig;
		private static Config.Processing.Icon IconConfig => ProcessingConfig.IconConfig;

		private const int SlotSize = 63;

		/// <summary>
		/// Static icons are those which are rendered ahead of time.
		/// For example keys, medical supply's, containers, standalone mods,
		/// and especially items like screws, drill, wires, milk and so on.
		/// <para/> Dictionary&lt;slotSize, Dictionary&lt;iconKey, icon&gt;&gt;
		/// </summary>
		private static Dictionary<Size, Dictionary<string, Mat>> StaticIcons = new();

		/// <summary>
		/// Dynamic icons are those which need to be rendered at runtime
		/// due to the items appearance being altered by attached items.
		/// For example weapons are considered dynamic items since their
		/// icon changes when you add rails, magazines, scopes and so on.
		/// <para/> Dictionary&lt;slotSize, Dictionary&lt;iconKey, icon&gt;&gt;
		/// </summary>
		private static Dictionary<Size, Dictionary<string, Mat>> DynamicIcons = new();

		/// <summary>
		/// The data used to match icons to their uid's.
		/// <para/> Dictionary&lt;iconKey, HashSet&lt;ItemInfo&gt;&gt;
		/// </summary>
		/// <remarks>
		/// Keep this synchronized with <see cref="CorrelationDataInv"/>
		/// </remarks>
		private static readonly Dictionary<string, HashSet<ItemInfo>> CorrelationData = new();

		/// <summary>
		/// The data used to match uid's to their icon.
		/// This is the inverse of <see cref="CorrelationData"/>
		/// <para/> Dictionary&lt;ItemInfo, iconKey&gt;
		/// </summary>
		/// <remarks>
		/// Keep this synchronized with <see cref="CorrelationData"/>
		/// </remarks>
		private static readonly Dictionary<ItemInfo, string> CorrelationDataInv = new();

		/// <summary>
		/// The icon paths connected to each icon key
		/// <para/> Dictionary&lt;iconKey, iconPath&gt;
		/// </summary>
		private static readonly Dictionary<string, string> IconPaths = new();

		private const int RetryCount = 3;
		private static int dynamicCorrelationDataHash;

		/// <summary>
		/// Get static icons with matching size
		/// </summary>
		/// <seealso cref="StaticIcons"/>
		/// <seealso cref="DynamicIcons"/>
		/// <param name="size">Size of the icon in cells</param>
		/// <returns>Dictionary with the key being the icon name and the value being the icon</returns>
		public static Dictionary<string, Mat> GetStaticIcons(Size size)
		{
			if (!StaticIcons.ContainsKey(size)) return null;
			return StaticIcons[size];
		}

		/// <summary>
		/// Get dynamic icons with matching size
		/// </summary>
		/// <seealso cref="StaticIcons"/>
		/// <seealso cref="DynamicIcons"/>
		/// <param name="size">Size of the icon in cells</param>
		/// <returns>Dictionary with the key being the uid and the value being the icon</returns>
		public static Dictionary<string, Mat> GetDynamicIcons(Size size)
		{
			if (!DynamicIcons.ContainsKey(size)) return null;
			return DynamicIcons[size];
		}

		/// <summary>
		/// Load all icons from path and organize them based on size
		/// </summary>
		public static void Init()
		{
			LoadStaticIcons();
			LoadStaticCorrelationData();
			InverseCorrelationData();

			if (IconConfig.UseDynamicIcons)
			{
				OnDynamicCorrelationDataChange(null, null);
				InitFileWatcher();
			}
		}

		#region Icon loading

		private static void LoadStaticIcons()
		{
			Logger.LogInfo("Loading static icons...");
			if (!Directory.Exists(PathConfig.StaticIcon))
			{
				var message = "Could not find icon folder at: " + PathConfig.StaticIcon;
				var ex = new ArgumentException(message, PathConfig.StaticIcon);
				Logger.LogError(ex);
				throw ex;
			}

			var iconPathArray = Directory.GetFiles(PathConfig.StaticIcon, "*.png");

			var loadedIcons = 0;
			var totalIcons = iconPathArray.Length;

			Parallel.ForEach(iconPathArray, iconPath =>
			{
				var fileName = Path.GetFileNameWithoutExtension(iconPath);
				var iconKey = GetIconKey(fileName, true);
				var mat = Cv2.ImRead(iconPath, ImreadModes.Unchanged);

				// We use a hardcoded slotSize since the icons, extracted from the game, are all FHD
				if (!IsValidPixelSize(mat.Width, SlotSize) || !IsValidPixelSize(mat.Height, SlotSize))
				{
					Logger.LogWarning("Icon has invalid size. Path: " + iconPath);
					return;
				}

				var size = new Size(PixelsToSlots(mat.Width, SlotSize), PixelsToSlots(mat.Height, SlotSize));

				lock (StaticIcons)
				{
					if (!StaticIcons.ContainsKey(size))
					{
						StaticIcons.Add(size, new Dictionary<string, Mat>());
					}

					// Add icon to icon and path dictionary
					StaticIcons[size][iconKey] = mat;
					IconPaths[iconKey] = iconPath;
					loadedIcons++;
				}
			});

			Logger.LogInfo("Loaded " + loadedIcons + "/" + totalIcons + " icons successfully.");
		}

		private static Dictionary<Size, Dictionary<string, Mat>> LoadDynamicIcons(int retryIndex = 0)
		{
			Logger.LogInfo("Loading dynamic icons...");
			var newDynamicIcons = new Dictionary<Size, Dictionary<string, Mat>>();

			if (!Directory.Exists(PathConfig.DynamicIcon))
			{
				var message = "Could not find icon cache folder at: " + PathConfig.DynamicIcon;
				var ex = new ArgumentException(message, PathConfig.DynamicIcon);
				Logger.LogError(ex);
				throw ex;
			}

			try
			{
				var iconPathArray = Directory.GetFiles(PathConfig.DynamicIcon, "*.png");

				var loadedIcons = 0;
				var totalIcons = iconPathArray.Length;

				Parallel.ForEach(iconPathArray, iconPath =>
				{
					var fileName = Path.GetFileNameWithoutExtension(iconPath);
					var iconKey = GetIconKey(fileName, false);
					var mat = Cv2.ImRead(iconPath, ImreadModes.Unchanged);

					// We use a hardcoded slotSize since the icons, extracted from the game, are all FHD
					if (!IsValidPixelSize(mat.Width, SlotSize) || !IsValidPixelSize(mat.Height, SlotSize))
					{
						Logger.LogWarning("Icon has invalid size. Path: " + iconPath);
						return;
					}

					var size = new Size(PixelsToSlots(mat.Width, SlotSize), PixelsToSlots(mat.Height, SlotSize));

					lock (newDynamicIcons)
					{
						if (!newDynamicIcons.ContainsKey(size))
						{
							newDynamicIcons.Add(size, new Dictionary<string, Mat>());
						}

						// Add icon to icon and path dictionary
						newDynamicIcons[size][iconKey] = mat;
						IconPaths[iconKey] = iconPath;
						loadedIcons++;
					}
				});

				Logger.LogInfo("Loaded " + loadedIcons + "/" + totalIcons + " icons successfully.");
				return newDynamicIcons;
			}
			catch (Exception e)
			{
				retryIndex++;
				Logger.LogWarning($"Could not load dynamic icons! ({retryIndex}/{RetryCount})", e);
				if (retryIndex < RetryCount)
				{
					Logger.LogInfo("Trying again...");
					Thread.Sleep(100);
					return LoadDynamicIcons(retryIndex);
				}

				return null;
			}
		}

		#endregion

		#region Correlation data loading

		private static void LoadStaticCorrelationData()
		{
			Logger.LogInfo("Loading static correlation data...");

			if (!File.Exists(PathConfig.StaticCorrelation))
			{
				var message = "Could not find static correlation data at: " + PathConfig.StaticCorrelation;
				var ex = new ArgumentException(message, PathConfig.StaticCorrelation);
				Logger.LogError(ex);
				throw ex;
			}

			var json = File.ReadAllText(PathConfig.StaticCorrelation);
			var correlations = JArray.Parse(json);

			foreach (var jToken in correlations)
			{
				var correlation = (JObject)jToken;
				var icon = correlation.GetValue("icon").ToString();
				var uid = correlation.GetValue("uid").ToString();

				// Remove file extension from icon path
				var fileName = Path.GetFileNameWithoutExtension(icon);
				var iconKey = GetIconKey(fileName, true);
				if (!CorrelationData.ContainsKey(iconKey))
				{
					CorrelationData.Add(iconKey, new HashSet<ItemInfo>());
				}

				// This will never throw because we just made sure that the key exists
				CorrelationData[iconKey].Add(new ItemInfo(uid));
			}
		}

		private static Dictionary<string, HashSet<ItemInfo>> LoadDynamicCorrelationData(int retryIndex = 0)
		{
			Logger.LogInfo("Loading dynamic correlation data...");
			var newCorrelationData = new Dictionary<string, HashSet<ItemInfo>>();

			if (!File.Exists(PathConfig.DynamicCorrelation))
			{
				var message = "Could not find dynamic correlation data at: " + PathConfig.DynamicCorrelation;
				var ex = new ArgumentException(message, PathConfig.DynamicCorrelation);
				Logger.LogError(ex);
				throw ex;
			}

			try
			{
				var json = File.ReadAllText(PathConfig.DynamicCorrelation);

				// Check if we already parsed this
				var hashCode = json.GetHashCode();
				if (hashCode == dynamicCorrelationDataHash)
				{
					Logger.LogInfo("Dynamic correlation data already up to date");
					return null;
				}

				dynamicCorrelationDataHash = hashCode;

				// We did not parse it earlier so we will do now
				var correlations = JObject.Parse(json);

				foreach (var correlation in correlations.Properties())
				{
					// Does not include extension already
					var fileName = correlation.Value.ToString();

					var uidString = correlation.Name;

					// Remove file extension from icon path
					var iconKey = GetIconKey(fileName, false);
					if (!newCorrelationData.ContainsKey(iconKey))
					{
						newCorrelationData.Add(iconKey, new HashSet<ItemInfo>());
					}

					// This will never throw because we just made sure that the key exists
					newCorrelationData[iconKey].Add(ParseUidString(uidString));
				}

				return newCorrelationData;
			}
			catch (Exception e)
			{
				retryIndex++;
				Logger.LogWarning($"Could not load dynamic correlation data! ({retryIndex}/{RetryCount})", e);
				if (retryIndex < RetryCount)
				{
					Logger.LogInfo("Trying again...");
					Thread.Sleep(100);
					return LoadDynamicCorrelationData(retryIndex);
				}

				return null;
			}
		}

		private static ItemInfo ParseUidString(string uidString)
		{
			var pairs = uidString.Split(',').Select(s => s.Trim()).ToArray();
			pairs = pairs.Where(pair => !string.IsNullOrEmpty(pair)).ToArray();

			// Remove and store leading base uid
			var space = new[] { ' ' };
			var baseUid = pairs[0].Split(space, 2)[0].Trim();
			pairs[0] = pairs[0].Split(space, 2)[1].Trim();

			var dDot = new[] { ':' };
			var mods = pairs.Select(s => new KeyValuePair<string, string>(s.Split(dDot, 2)[0], s.Split(dDot, 2)[1]));

			var modUidList = new List<string>();
			foreach (var mod in mods)
			{
				var modUid = mod.Value.Split(dDot, 2)[0].Trim();
				modUidList.Add(modUid);
			}

			var cleanModUidList = modUidList.Where(uid => !string.IsNullOrEmpty(uid) && uid.Length > 12).ToArray();
			return new ItemInfo(baseUid, cleanModUidList, uidString);
		}

		/// <summary>
		/// Inverse the correlation data to have quick access to it like a bidirectional dictionary
		/// </summary>
		private static void InverseCorrelationData()
		{
			// Clear in inverse data
			CorrelationDataInv.Clear();

			// Populate with new data
			foreach (var entry in CorrelationData)
			{
				foreach (var itemInfo in entry.Value)
				{
					CorrelationDataInv[itemInfo] = entry.Key;
				}
			}
		}

		#endregion

		/// <summary>
		/// Initialize a file watcher for the dynamic correlation data
		/// to update the dynamic icons when something changes
		/// </summary>
		private static void InitFileWatcher()
		{
			Logger.LogInfo("Initializing file watcher for icon cache...");
			var fileWatcher = new FileSystemWatcher();
			fileWatcher.Path = Path.GetDirectoryName(PathConfig.DynamicCorrelation);
			fileWatcher.Filter = Path.GetFileName(PathConfig.DynamicCorrelation);
			fileWatcher.NotifyFilter = NotifyFilters.Size;
			fileWatcher.Changed += OnDynamicCorrelationDataChange;
			fileWatcher.EnableRaisingEvents = true;
		}

		/// <summary>
		/// Event which gets called when the dynamic correlation data file
		/// get changed. Dynamic icons and correlation data get updated.
		/// </summary>
		private static async void OnDynamicCorrelationDataChange(object source, FileSystemEventArgs e)
		{
			Logger.LogDebug("Dynamic correlation data changed");
			if (!IconConfig.UseDynamicIcons) return;

			// Wait if currently scanning a item
			//while (Config.General.ProcessingLock) await Task.Delay(25);

			// Try to load new dynamic correlation data
			var newDynamicCorrelationData = LoadDynamicCorrelationData();
			if (newDynamicCorrelationData == null) return;

			// Try to load new dynamic icons
			var newDynamicIcons = LoadDynamicIcons();
			if (newDynamicIcons == null) return;

			// All data loaded successfully at this point so we can begin swapping the data

			// Acquire the lock
			//Config.General.ProcessingLock = true;

			// Replace old with new dynamic icons
			DynamicIcons = newDynamicIcons;

			// Replace old with new dynamic correlation data
			foreach (var entry in newDynamicCorrelationData)
			{
				CorrelationData[entry.Key] = entry.Value;
			}

			// Update inverse correlation data
			InverseCorrelationData();

			// Release the lock
			//Config.General.ProcessingLock = false;
		}

		/// <summary>
		/// Get the amount of icons in the icon cache folder
		/// </summary>
		/// <returns>Count of icons in the icon cache folder</returns>
		public static int GetIconCacheSize()
		{
			if (!Directory.Exists(PathConfig.DynamicIcon))
			{
				throw new FileNotFoundException("Could not find icon cache folder", PathConfig.DynamicIcon);
			}

			return Directory.GetFiles(PathConfig.DynamicIcon, "*.png").Length;
		}

		/// <summary>
		/// Delete the icon cache folder
		/// </summary>
		public static void ClearIconCache()
		{
			try
			{
				var iconPathArray = Directory.GetFiles(PathConfig.DynamicIcon, "*.png");
				foreach (var iconPath in iconPathArray) File.Delete(iconPath);
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
			catch (Exception e)
			{
				Logger.LogWarning("Could not delete icon cache folder", e);
			}

			File.WriteAllText(PathConfig.DynamicCorrelation, "{}");
		}

		/// <summary>
		/// Get item info's associated to a icon key
		/// </summary>
		/// <param name="iconKey">The icon key used to find the item info's</param>
		/// <returns>Item info's associated with the icon key</returns>
		public static ItemInfo[] GetItemInfo(string iconKey)
		{
			if (iconKey == null) return null;
			var success = CorrelationData.TryGetValue(iconKey, out var itemInfo);
			if (success) return itemInfo.ToArray();

			Logger.LogWarning("Could not find any item info for icon key: " + iconKey);
			return null;
		}

		/// <summary>
		/// Compute the icon key of a give icon
		/// </summary>
		/// <param name="fileName">The file name of the icon without extension</param>
		/// <param name="isStaticIcon">
		/// True if the item is rendered ahead of time.
		/// <seealso cref="StaticIcons"/>
		/// <seealso cref="DynamicIcons"/>
		/// </param>
		/// <returns>The icon key</returns>
		private static string GetIconKey(string fileName, bool isStaticIcon)
		{
			return fileName + (isStaticIcon ? "-Static" : "-Dynamic");
			//var sBuilder = new StringBuilder();
			//var buffer = Encoding.UTF8.GetBytes(fileName + isStaticIcon);
			//using (var hash = SHA256.Create())
			//{
			//    var result = hash.ComputeHash(buffer);
			//    foreach (var b in result) sBuilder.Append(b.ToString("x2"));
			//}
			//return sBuilder.ToString();
		}

		public static string GetIconPath(ItemInfo itemInfo)
		{
			var success = CorrelationDataInv.TryGetValue(itemInfo, out var iconKey);
			if (!success)
			{
				Logger.LogWarning("Could not find icon key for:\n" + itemInfo);
				return PathConfig.UnknownIcon;
			}

			success = IconPaths.TryGetValue(iconKey, out var path);
			if (!success)
			{
				Logger.LogWarning("Could not find path for icon key: " + iconKey);
				return PathConfig.UnknownIcon;
			}

			if (!File.Exists(path))
			{
				Logger.LogWarning("Could not find icon for: " + itemInfo.Uid + "\nat: " + path);
				return PathConfig.UnknownIcon;
			}

			return path;
		}

		/// <summary>
		/// Converts the pixel unit of a icons into the slot unit
		/// </summary>
		/// <param name="pixels">The pixel size of the icon</param>
		/// <param name="slotSize">Slot size to use for conversion</param>
		/// <returns>Slot size of the icon</returns>
		public static int PixelsToSlots(float pixels, float? slotSize = null)
		{
			// Use converter class to round to nearest int instead of always rounding down
			return Convert.ToInt32((pixels - 1) / (slotSize ?? ProcessingConfig.ScaledSlotSize));
		}

		/// <summary>
		/// Converts the slot unit of a icons into the pixel unit
		/// </summary>
		/// <param name="slots">The slot size of the icon</param>
		/// <param name="slotSize">Slot size to use for conversion</param>
		/// <returns>Pixel size of the icon</returns>
		public static float SlotsToPixels(int slots, float? slotSize = null)
		{
			return slots * (slotSize ?? ProcessingConfig.ScaledSlotSize) + 1;
		}

		/// <summary>
		/// Checks if the give pixels can be converted into slot unit
		/// </summary>
		/// <param name="pixels">The pixel size of the icon</param>
		/// <param name="slotSize">The slot size of the icon</param>
		/// <returns>True if the pixels can be converted to slots</returns>
		private static bool IsValidPixelSize(float pixels, float? slotSize = null)
		{
			return Math.Abs(1 - pixels % (slotSize ?? ProcessingConfig.ScaledSlotSize)) < 0.1f;
		}
	}
}
