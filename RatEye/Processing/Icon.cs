using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using RatStash;
using static RatEye.Config.Processing;

namespace RatEye.Processing
{
	public class Icon
	{
		private readonly Config _config;
		private readonly Bitmap _icon;
		private Bitmap _scaledIcon;
		private Item _item;
		private ItemExtraInfo _itemExtraInfo;
		private float _detectionConfidence;
		private Vector2 _itemPosition;
		private bool _rotated;

		private Config.Processing ProcessingConfig => _config.ProcessingConfig;
		private Config.Processing.Icon IconConfig => ProcessingConfig.IconConfig;

		/// <summary>
		/// Sync object to synchronize control flow of multiple threads
		/// </summary>
		private object _sync = new();

		/// <summary>
		/// Position of the icon inside the inventory
		/// </summary>
		public Vector2 Position { get; }

		/// <summary>
		/// Size of the icon image, measured in pixel
		/// </summary>
		/// <remarks>
		/// This might not be the exact size of the item and does not account for rotation
		/// </remarks>
		public Vector2 Size { get; }

		/// <summary>
		/// Size of the item, measured in pixels
		/// </summary>
		public Vector2 ItemSize
		{
			get
			{
				var size = IconSlotSize();
				size.X = (int)(size.X * ProcessingConfig.ScaledSlotSize);
				size.Y = (int)(size.Y * ProcessingConfig.ScaledSlotSize);
				return size;
			}
		}

		/// <summary>
		/// The detected item
		/// </summary>
		public Item Item
		{
			get
			{
				SatisfyState(State.Scanned);
				return _item;
			}
		}

		/// <summary>
		/// The detected item extra info
		/// </summary>
		public ItemExtraInfo ItemExtraInfo
		{
			get
			{
				SatisfyState(State.Scanned);
				return _itemExtraInfo;
			}
		}

		/// <summary>
		/// The path to the icon of the detected item
		/// </summary>
		public string IconPath => _config.IconManager.GetIconPath(Item, ItemExtraInfo ?? new ItemExtraInfo());

		/// <summary>
		/// Confidence with which the <see cref="Item"/> was detected/>
		/// </summary>
		public float DetectionConfidence
		{
			get
			{
				SatisfyState(State.Scanned);
				return _detectionConfidence;
			}
		}

		/// <summary>
		/// The exact position of the item
		/// </summary>
		public Vector2 ItemPosition
		{
			get
			{
				SatisfyState(State.Scanned);
				return _itemPosition;
			}
		}

		/// <summary>
		/// <see langword="true"/> if the icon is rotated
		/// </summary>
		public bool Rotated
		{
			get
			{
				SatisfyState(State.Scanned);
				return _rotated;
			}
		}

		internal Icon(Bitmap icon, Vector2 position, Vector2 size, Config config)
		{
			_config = config;
			_icon = icon;
			Position = position;
			Size = size;
		}

		private enum State
		{
			Default,
			Rescaled,
			Scanned,
		}

		private State _currentState = State.Default;

		private void SatisfyState(State targetState)
		{
			while (_currentState < targetState)
			{
				switch (_currentState + 1)
				{
					case State.Default:
						break;
					case State.Rescaled:
						RescaleIcon();
						break;
					case State.Scanned:
						TemplateMatch();
						if (_config.ProcessingConfig.IconConfig.ScanRotatedIcons) TemplateMatch(true);
						break;
					default:
						throw new Exception("Cannot satisfy unknown state.");
				}

				_currentState++;
			}
		}

		private void RescaleIcon()
		{
			Logger.LogDebugBitmap(_icon, "icon/_icon");
			_scaledIcon = _icon.Rescale(ProcessingConfig.InverseScale);
		}

		private void TemplateMatch(bool rotated = false)
		{
			SatisfyState(State.Rescaled);

			// NOTE The source image is scaled hence all outgoing pixel values need to be adjusted accordingly
			using var source = _scaledIcon.ToMat();
			if (rotated) Cv2.Rotate(source, source, RotateFlags.Rotate90Counterclockwise);

			Logger.LogDebugMat(source, "icon/source");

			(string match, float confidence, Vector2 pos) staticResult = default;
			(string match, float confidence, Vector2 pos) dynamicResult = default;

			if (!(IconConfig.UseStaticIcons || IconConfig.UseDynamicIcons))
			{
				throw new Exception(
					"No icons for template matching can be used. At least one of " +
					nameof(IconConfig.UseStaticIcons) + " and " + nameof(IconConfig.UseDynamicIcons) +
					" have to be set");
			}

			var iconManager = _config.IconManager;
			var iconSlotSize = IconSlotSize();
			var slotSize = rotated ? new Vector2(iconSlotSize.Y, iconSlotSize.X) : iconSlotSize;
			if (IconConfig.UseStaticIcons)
			{
				if (iconManager.StaticIcons.ContainsKey(slotSize))
				{
					iconManager.StaticIconsLock.EnterReadLock();
					try { staticResult = TemplateMatchSub(source, iconManager.StaticIcons[slotSize]); }
					finally { iconManager.StaticIconsLock.ExitReadLock(); }
				}
			}

			if (IconConfig.UseDynamicIcons)
			{
				if (iconManager.DynamicIcons.ContainsKey(slotSize))
				{
					iconManager.DynamicIconsLock.EnterReadLock();
					try { dynamicResult = TemplateMatchSub(source, iconManager.DynamicIcons[slotSize]); }
					finally { iconManager.DynamicIconsLock.ExitReadLock(); }
				}
			}

			var result = staticResult.confidence > dynamicResult.confidence ? staticResult : dynamicResult;

			if (!(result.confidence > _detectionConfidence)) return;

			_rotated = rotated;
			_itemPosition = (rotated ? new(result.pos.Y, result.pos.X) : result.pos) * _config.ProcessingConfig.Scale;
			_detectionConfidence = result.confidence;
			_item = _config.IconManager.GetItem(result.match);
			_itemExtraInfo = _config.IconManager.GetItemExtraInfo(result.match);
		}

		private (string match, float confidence, Vector2 pos) TemplateMatchSub(Mat source, Dictionary<string, Mat> icons)
		{
			var bestMatch = "";
			var confidence = 0f;
			var position = Vector2.Zero;

			Parallel.ForEach(icons, icon =>
			{
				using var matches = source.MatchTemplate(icon.Value, TemplateMatchModes.SqDiffNormed);
				matches.MinMaxLoc(out var minVal, out _, out var minLoc, out _);

				lock (_sync)
				{
					minVal = 1 - minVal;
					if (!(minVal > confidence)) return;
					confidence = (float)minVal;
					bestMatch = icon.Key;
					position = new Vector2(minLoc);
					//Logger.LogDebugMat(icon.Value, $"icon/conf-{confidence}.png");
					//Logger.LogDebugMat(matches, $"icon/match-conf-{confidence}.png");
				}
			});

			return (bestMatch, confidence, position);
		}

		/// <summary>
		/// Converts the pixel unit of the icon into the slot unit
		/// </summary>
		/// <returns>Slot size of the icon</returns>
		private Vector2 IconSlotSize()
		{
			// Use converter class to round to nearest int instead of always rounding down
			var x = (Size.X - 1) / ProcessingConfig.ScaledSlotSize;
			var y = (Size.Y - 1) / ProcessingConfig.ScaledSlotSize;
			return new Vector2((int)x, (int)y);
		}
	}
}
