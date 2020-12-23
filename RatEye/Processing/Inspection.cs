using System;
using System.Drawing;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Text;
using InspectionConfig = RatEye.Config.Processing.Inspection;

namespace RatEye.Processing
{
	public class Inspection
	{
		private Bitmap _image;
		private static OCRTesseract tesseractInstance;

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
		private float _markerConfidence = 0f;
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
		/// <remarks>Null if no marker above threshold found</remarks>
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
		/// True if the provided image contains a marker
		/// </summary>
		public bool ContainsMarker => MarkerPosition != null;

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
		/// Constructor for inspection view processing object
		/// </summary>
		/// <param name="image">Image of the inspection view which will be processed</param>
		/// <remarks>Provided image has to be in RGB</remarks>
		public Inspection(Bitmap image)
		{
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
				try
				{
					switch (++_currentState)
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
				}
				catch (Exception e)
				{
					Logger.LogError(e);
					throw;
				}
			}
		}
		#endregion

		/// <summary>
		/// Search for all different marker types and pick the best matching one
		/// </summary>
		private void SearchMarker()
		{
			var item = GetMarkerPosition(InspectionConfig.GetScaledMarker(InspectionType.Item));
			var container = GetMarkerPosition(InspectionConfig.GetScaledMarker(InspectionType.Container));

			if (item.confidence > container.confidence)
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
			using (var refMat = _image.ToMat()) // ref = reference
			using (var tplMat = marker.ToMat()) // tpl = template
			using (var res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
			{
				// Gray scale both reference and template image
				var gref = refMat.CvtColor(ColorConversionCodes.RGB2GRAY);
				var gtpl = tplMat.CvtColor(ColorConversionCodes.RGB2GRAY);

				Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
				//Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);
				Cv2.MinMaxLoc(res, out _, out var maxVal, out _, out var maxLoc);

				return ((float)maxVal, new Vector2(maxLoc));
			}
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
			position.X += InspectionConfig.GetHorizontalTitleSearchOffset(DetectedInspectionType);

			// Find end of the title bar
			var scaledMarker = InspectionConfig.GetScaledMarker(DetectedInspectionType);
			var closeBtnCenterHeight = MarkerPosition.Y + (scaledMarker.Height / 2);
			var lowC = InspectionConfig.CloseButtonColorLowerBound;
			var upC = InspectionConfig.CloseButtonColorUpperBound;
			var closeButtonPosition = _image.FindPixelInRange(closeBtnCenterHeight, lowC, upC, position.X);

			// Construct final search box dimensions
			var height = Math.Min(InspectionConfig.ScaledTitleSearchHeight, _image.Height - position.Y);
			var width = Math.Min(InspectionConfig.ScaledTitleSearchWidth, _image.Width - position.X);
			if (closeButtonPosition > 0)
			{
				// Calculate width of search box to the close button edge
				var tmpWidth = closeButtonPosition - position.X;
				// Shorten the width to account for extra buttons ( for example sort buttons )
				tmpWidth -= InspectionConfig.ScaledTitleSearchRightPadding;
				// Apply new width if its the new minimum width
				width = Math.Min(width, tmpWidth);
			}

			// Crop image to title search box
			var searchBox = _image.Crop(position.X, position.Y, width, height);

			// Rescale title search box to 4k, adjusting the font size to the training data
			// We multiply the inverse scale with 2f to rescale to 4k instead of 1080p
			var rescaledSearchBox = searchBox.Rescale(Config.Processing.InverseScale * 2f);

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
		private static OCRTesseract GetTesseractInstance()
		{
			if (tesseractInstance != null) return tesseractInstance;

			// Check if traineddata is present
			var traineddataPath = Path.Combine(Config.Path.BenderTraineddata, "bender.traineddata");
			if (!File.Exists(traineddataPath))
			{
				var message = "Could not find traineddata at: " + traineddataPath;
				var ex = new ArgumentException(message, Config.Path.BenderTraineddata);
				Logger.LogError(ex);
				throw ex;
			}

			// Create a tesseract instance
			try
			{
				var datapath = Config.Path.BenderTraineddata;
				var language = "bender";

				tesseractInstance = OCRTesseract.Create(datapath, language, null, 3, 7);
			}
			catch (Exception e)
			{
				Logger.LogError("Could not create tesseract instance!", e);
				throw;
			}

			return tesseractInstance;
		}
	}
}
