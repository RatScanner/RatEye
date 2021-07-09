using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using RatStash;

namespace RatEye.Processing
{
	public class Icon
	{
		private readonly Config _config;
		private readonly Bitmap _icon;
		private Bitmap _scaledIcon;
		private Item _item;
		private float _detectionConfidence;
		private Vector2 _itemPosition;

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
		/// Size of the icon, measured in pixel
		/// </summary>
		public Vector2 Size { get; }

		/// <summary>
		/// Size of the icon, measured in slots
		/// </summary>
		public Vector2 SlotSize => PixelsToSlots();

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
			_scaledIcon = _icon.Rescale(ProcessingConfig.InverseScale);
		}

		private void TemplateMatch(bool rotated = false)
		{
			SatisfyState(State.Rescaled);

			var source = _scaledIcon.ToMat();
			if (rotated)
			{
				var rotatedIconMat = new Mat();
				Cv2.Rotate(source, rotatedIconMat, RotateFlags.Rotate90Counterclockwise);
				source = rotatedIconMat;
			}

			(string match, float confidence, Vector2 pos) staticResult = default;
			(string match, float confidence, Vector2 pos) dynamicResult = default;

			if (!(IconConfig.UseStaticIcons && IconConfig.UseDynamicIcons))
			{
				throw new Exception(
					"No icons for template matching can be used. At least one of " +
					nameof(IconConfig.UseStaticIcons) + " and " + nameof(IconConfig.UseDynamicIcons) +
					" have to be set");
			}

			var iconManager = _config.IconManager;
			var slotSize = rotated ? new Vector2(SlotSize.Y, SlotSize.X) : SlotSize;
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

			if (_detectionConfidence > result.confidence) return;

			_itemPosition = result.pos;
			_detectionConfidence = result.confidence;
			_item = _config.IconManager.GetItem(result.match);
		}

		private (string match, float confidence, Vector2 pos) TemplateMatchSub(Mat source, Dictionary<string, Mat> icons)
		{
			var bestMatch = "";
			var confidence = 0f;
			var position = Vector2.Zero;

			Parallel.ForEach(icons, icon =>
			{
				// TODO Prepare masks when loading icons
				var mask = icon.Value.InRange(new Scalar(0, 0, 0, 128), new Scalar(255, 255, 255, 255));
				mask = mask.CvtColor(ColorConversionCodes.GRAY2BGR, 3);
				var iconNoAlpha = icon.Value.CvtColor(ColorConversionCodes.BGRA2BGR, 3);

				var matches = source.MatchTemplate(iconNoAlpha, TemplateMatchModes.CCorrNormed, mask);
				matches.MinMaxLoc(out _, out var maxVal, out _, out var maxLoc);

				lock (_sync)
				{
					if (!(maxVal > confidence)) return;
					confidence = (float)maxVal;
					bestMatch = icon.Key;
					position = new Vector2(maxLoc);
				}
			});

			return (bestMatch, confidence, position);
		}

		/// <summary>
		/// Converts the pixel unit of the icon into the slot unit
		/// </summary>
		/// <returns>Slot size of the icon</returns>
		internal Vector2 PixelsToSlots()
		{
			// Use converter class to round to nearest int instead of always rounding down
			var x = (Size.X - 1) / ProcessingConfig.ScaledSlotSize;
			var y = (Size.Y - 1) / ProcessingConfig.ScaledSlotSize;
			return new Vector2(Convert.ToInt32(x), Convert.ToInt32(y));
		}
	}
}
