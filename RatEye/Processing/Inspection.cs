using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Text;
using RatStash;

namespace RatEye.Processing
{
	public class Inspection
	{
		private readonly Config _config;
		private readonly Bitmap _image;
		private static OCRTesseract _tesseractInstance;

		private Config.Path PathConfig => _config.PathConfig;
		private Config.Processing ProcessingConfig => _config.ProcessingConfig;
		private Config.Processing.Inspection InspectionConfig => ProcessingConfig.InspectionConfig;

		/// <summary>
		/// Inspection window variants
		/// </summary>
		public enum InspectionType
		{
			/// <summary>Unknown inspection type</summary>
			Unknown,

			/// <summary>When double clicking a item or clicking the inspect tooltip</summary>
			Item,

			/// <summary>When opening a container like a Keytool or a Scavbox</summary>
			Container,
		}

		// Backing property fields
		private InspectionType _detectedInspectionType = InspectionType.Unknown;
		private Vector2 _markerPosition;
		private float _markerConfidence;
		private string _title = "";

		/// <summary>
		/// Detected type of the inspection window
		/// </summary>
		public InspectionType DetectedInspectionType
		{
			get
			{
				SatisfyState(State.SearchedMarker);
				return _detectedInspectionType;
			}
			private set => _detectedInspectionType = value;
		}

		/// <summary>
		/// Position of the marker in the given image
		/// </summary>
		/// <remarks><see langword="null"/> if no marker above threshold found</remarks>
		public Vector2 MarkerPosition
		{
			get
			{
				SatisfyState(State.SearchedMarker);
				return _markerPosition;
			}
			private set => _markerPosition = value;
		}

		/// <summary>
		/// The confidence that the image contains a marker
		/// </summary>
		public float MarkerConfidence
		{
			get
			{
				SatisfyState(State.SearchedMarker);
				return _markerConfidence;
			}
			private set => _markerConfidence = value;
		}

		/// <summary>
		/// <see langword="true"/> if the provided image contains a marker above the threshold
		/// </summary>
		public bool ContainsMarker => MarkerConfidence >= InspectionConfig.MarkerThreshold;

		/// <summary>
		/// Title of the inspection window
		/// <para>The title which is to the right of the marker</para>
		/// </summary>
		public string Title
		{
			get
			{
				SatisfyState(State.ScannedTitle);
				return _title;
			}
			private set => _title = value;
		}

		/// <summary>
		/// Detected item
		/// </summary>
		public Item Item
		{
			get
			{
				SatisfyState(State.ScannedTitle);
				return GetItem();
			}
		}

		/// <summary>
		/// The path to the icon of the detected item
		/// </summary>
		public string IconPath => _config.IconManager.GetIconPath(Item, new ItemExtraInfo());

		/// <summary>
		/// Constructor for inspection view processing object
		/// </summary>
		/// <param name="image">Image of the inspection view which will be processed</param>
		/// <param name="overrideConfig">When provided, will be used instead of <see cref="Config.GlobalConfig"/></param>
		/// <remarks>Provided image has to be in RGB</remarks>
		public Inspection(Bitmap image, Config overrideConfig = null)
		{
			_config = overrideConfig ?? Config.GlobalConfig;
			_image = image;
		}

		#region Processing state handling

		private enum State
		{
			Default,
			SearchedMarker,
			ScannedTitle,
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
					case State.SearchedMarker:
						SearchMarker();
						break;
					case State.ScannedTitle:
						ScanTitle();
						break;
					default:
						throw new Exception("Cannot satisfy unknown state.");
				}

				_currentState++;
			}
		}

		#endregion

		/// <summary>
		/// Search for all different marker types and pick the best matching one
		/// </summary>
		private void SearchMarker()
		{
			SatisfyState(State.Default);

			var item = GetMarkerPosition(GetScaledMarker(InspectionType.Item));

			(float confidence, Vector2 position) container = (-1f, Vector2.Zero);
			if (_config.ProcessingConfig.InspectionConfig.EnableContainers)
			{
				container = GetMarkerPosition(GetScaledMarker(InspectionType.Container));
			}

			if (item.confidence >= container.confidence)
			{
				DetectedInspectionType = InspectionType.Item;
				MarkerConfidence = item.confidence;
				MarkerPosition = item.position;
			}
			else
			{
				DetectedInspectionType = InspectionType.Container;
				MarkerConfidence = container.confidence;
				MarkerPosition = container.position;
			}
		}

		/// <summary>
		/// Identify the give marker inside the source
		/// </summary>
		/// <param name="marker">The marker template to identify</param>
		/// <remarks>Provided marker has to be in RGB</remarks>
		/// <returns>Confidence and position of the best match</returns>
		private (float confidence, Vector2 position) GetMarkerPosition(Bitmap marker)
		{
			using var refMat = _image.ToMat();
			using var tplMat = marker.ToMat(); // tpl = template
			using var res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1);

			// Gray scale both reference and template image
			using var gref = refMat.CvtColor(ColorConversionCodes.RGB2GRAY);
			using var gtpl = tplMat.CvtColor(ColorConversionCodes.RGB2GRAY);

			Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
			//Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);
			Cv2.MinMaxLoc(res, out _, out var maxVal, out _, out var maxLoc);

			return ((float)maxVal, new Vector2(maxLoc));
		}

		private void ScanTitle()
		{
			if (!ContainsMarker)
			{
				// No marker? Why even bother scanning...
				Logger.LogDebug("No marker found!");
				return;
			}

			// Compute title search box dimensions
			var position = MarkerPosition;
			position.X += GetHorizontalTitleSearchOffset(DetectedInspectionType);

			// Find end of the title bar
			var scaledMarker = GetScaledMarker(DetectedInspectionType);
			var closeBtnCenterHeight = MarkerPosition.Y + (scaledMarker.Height / 2);
			var lowC = InspectionConfig.CloseButtonColorLowerBound;
			var upC = InspectionConfig.CloseButtonColorUpperBound;
			var closeButtonPosition = _image.FindPixelInRange(closeBtnCenterHeight, lowC, upC, position.X);

			// Construct final search box dimensions
			var scaledTitleSearchHeight = (int)(InspectionConfig.BaseTitleSearchHeight * ProcessingConfig.Scale);
			var scaledTitleSearchWidth = (int)(InspectionConfig.BaseTitleSearchWidth * ProcessingConfig.Scale);
			var height = Math.Min(scaledTitleSearchHeight, _image.Height - position.Y);
			var width = Math.Min(scaledTitleSearchWidth, _image.Width - position.X);
			if (closeButtonPosition > 0)
			{
				// Calculate width of search box to the close button edge
				var tmpWidth = closeButtonPosition - position.X;
				// Shorten the width to account for extra buttons ( for example sort buttons )
				var titleSearchRightPadding =
					(int)(InspectionConfig.BaseTitleSearchRightPadding * ProcessingConfig.Scale);
				tmpWidth -= titleSearchRightPadding;
				// Apply new width if its the new minimum width
				width = Math.Min(width, tmpWidth);
			}

			// Crop image to title search box
			var searchBox = _image.Crop(position.X, position.Y, width, height);

			// Rescale title search box to 4k, adjusting the font size to the training data
			// We multiply the inverse scale with 2f to rescale to 4k instead of 1080p
			var rescaledSearchBox = searchBox.Rescale(ProcessingConfig.InverseScale * 2f);

			Title = OCR(rescaledSearchBox);
		}

		/// <summary>
		/// Perform optical character recognition on a image
		/// </summary>
		/// <param name="image">Image to perform OCR on</param>
		/// <returns>Detected characters in image</returns>
		private string OCR(Bitmap image)
		{
			// Setup tesseract
			var text = "";
			using (var mat = image.ToMat())
			{
				// Gray scale image
				Logger.LogDebug("Gray scaling...");
				var cvu83 = mat.CvtColor(ColorConversionCodes.BGR2GRAY, 1);

				// Binarize image
				Logger.LogDebug("Binarizing...");
				cvu83 = cvu83.Threshold(120, 255, ThresholdTypes.BinaryInv);

				// OCR
				Logger.LogDebug("Applying OCR...");
				GetTesseractInstance().Run(cvu83, out text, out _, out _, out _, ComponentLevels.TextLine);
			}

			Logger.LogDebug("Read: " + text);
			return text;
		}

		/// <summary>
		/// Creates an instance of the OCRTesseract class. Initializes Tesseract.
		/// </summary>
		/// <returns>Tesseract instance trained for the bender font</returns>
		private OCRTesseract GetTesseractInstance()
		{
			// Return if tesseract instance was already created
			if (_tesseractInstance != null) return _tesseractInstance;

			// Check if trained data is present
			var traineddataPath = Path.Combine(PathConfig.BenderTraineddata, "bender.traineddata");
			if (!File.Exists(traineddataPath))
			{
				var message = "Could not find traineddata at: " + traineddataPath;
				var ex = new ArgumentException(message, PathConfig.BenderTraineddata);
				throw ex;
			}

			// Create a tesseract instance
			var datapath = PathConfig.BenderTraineddata;
			var language = "bender";

			_tesseractInstance = OCRTesseract.Create(datapath, language, null, 3, 7);
			return _tesseractInstance;
		}

		/// <summary>
		/// Generate a marker bitmap based on the inspection type
		/// </summary>
		/// <param name="inspectionType">The inspection type, determining the final marker scale</param>
		/// <remarks><see cref="Config.Processing.Scale"/> is already accounted for.</remarks>
		/// <returns>A rescaled and alpha blended version of <see cref="Config.Processing.Inspection.Marker"/></returns>
		private Bitmap GetScaledMarker(InspectionType inspectionType)
		{
			var markerScale = inspectionType switch
			{
				InspectionType.Item => InspectionConfig.MarkerItemScale,
				InspectionType.Container => InspectionConfig.MarkerContainerScale,
				InspectionType.Unknown => throw new InvalidOperationException(),
				_ => throw new ArgumentOutOfRangeException(nameof(inspectionType), inspectionType, null)
			};

			var output = InspectionConfig.Marker.Rescale(markerScale * ProcessingConfig.Scale);
			return output.TransparentToColor(InspectionConfig.MarkerBackgroundColor);
		}

		/// <summary>
		/// Scaled horizontal offset of the inspection window title search box
		/// <para>See <see cref="Config.Processing.Inspection.HorizontalTitleSearchOffsetFactor"/>.</para>
		/// </summary>
		/// <param name="inspectionType"></param>
		/// <returns>The distance between the right edge of the marker and the beginning of the title search box</returns>
		private int GetHorizontalTitleSearchOffset(InspectionType inspectionType)
		{
			var width = GetScaledMarker(inspectionType).Width;
			return (int)(width * InspectionConfig.HorizontalTitleSearchOffsetFactor);
		}

		/// <summary>
		/// Get the item, best matching the scanned title
		/// </summary>
		/// <returns>Item instance</returns>
		private Item GetItem()
		{
			var items = Config.RatStashDB.GetItems();
			return DetectedInspectionType switch
			{
				InspectionType.Item => items.Aggregate((i1, i2) =>
				{
					var i1Dist = i1.Name.NormedLevenshteinDistance(Title);
					var i2Dist = i2.Name.NormedLevenshteinDistance(Title);
					return i1Dist > i2Dist ? i1 : i2;
				}),
				InspectionType.Container => items.Aggregate((i1, i2) =>
				{
					var i1Dist = i1.ShortName.NormedLevenshteinDistance(Title);
					var i2Dist = i2.ShortName.NormedLevenshteinDistance(Title);
					return i1Dist > i2Dist ? i1 : i2;
				}),
				InspectionType.Unknown => null,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
