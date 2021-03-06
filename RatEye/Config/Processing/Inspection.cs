using System;
using System.Drawing;
using System.IO;
using RatEye.Properties;
using InspectionProcessing = RatEye.Processing.Inspection;

namespace RatEye
{
	public static partial class Config
	{
		public static partial class Processing
		{
			/// <summary>
			/// The Inspection class contains parameters, used by the inspection processing module
			/// </summary>
			public static class Inspection
			{
				/// <summary>
				/// Marker bitmap to identify regions of interest. This should be a cropped image of the magnifier icon
				/// </summary>
				public static Bitmap Marker = new(new MemoryStream(Resources.searchIcon));

				/// <summary>
				/// Detection threshold of the marker bitmap
				/// </summary>
				public static float MarkerThreshold = 0.9f;

				/// <summary>
				/// If true, allows to process the inspection window of containers
				/// </summary>
				public static bool EnableContainers = true;

				/// <summary>
				/// The scale of the marker used by item inspection windows
				/// </summary>
				public static float MarkerItemScale = 17f / 19f;

				/// <summary>
				/// The scale of the marker used by container inspection windows
				/// </summary>
				public static float MarkerContainerScale = 1f;

				/// <summary>
				/// The background color used for the marker if it uses a alpha channel
				/// </summary>
				public static Color MarkerBackgroundColor = Color.FromArgb(25, 27, 27);

				/// <summary>
				/// Unscaled width of the box which will be searched for the title
				/// </summary>
				public static int BaseTitleSearchWidth = 500;

				/// <summary>
				/// Scaled width of the box which will be searched for the title
				/// <para>See <see cref="BaseTitleSearchWidth"/></para>
				/// </summary>
				internal static int ScaledTitleSearchWidth => (int)(BaseTitleSearchWidth * Scale);

				/// <summary>
				/// Unscaled height of the box which will be searched for the title
				/// </summary>
				public static int BaseTitleSearchHeight = 17;

				/// <summary>
				/// Scaled height of the box which will be searched for the title
				/// <para>See <see cref="BaseTitleSearchHeight"/></para>
				/// </summary>
				internal static int ScaledTitleSearchHeight => (int)(BaseTitleSearchHeight * Scale);

				/// <summary>
				/// Right padding of the title search box, used when the close button got detected
				/// <para>
				/// This helps to ignore extra buttons like the sort buttons in some container inspection windows.
				/// </para>
				/// See <see cref="CloseButtonColorLowerBound"/> and <see cref="CloseButtonColorUpperBound"/>.
				/// </summary>
				public static int BaseTitleSearchRightPadding = 64;

				/// <summary>
				/// Scaled right padding of the box which will be searched for the title
				/// <para>See <see cref="BaseTitleSearchRightPadding"/></para>
				/// </summary>
				internal static int ScaledTitleSearchRightPadding => (int)(BaseTitleSearchRightPadding * Scale);

				/// <summary>
				/// The horizontal offset factor of the box which will be searched for the title
				/// <para>
				/// The factor is applied to the scaled width of the <see cref="Marker"/>.
				/// The horizontal offset is originating from the left most edge of the detected marker.
				/// <code>searchBox.left = detectedMarker.left + (Marker.width * Scale)</code>
				/// </para>
				/// </summary>
				public static float HorizontalTitleSearchOffsetFactor = 1.2f;

				/// <summary>
				/// Lower bound color to match the close button which is positioned at the top right of inspection windows
				/// </summary>
				public static Color CloseButtonColorLowerBound = Color.FromArgb(50, 10, 10);

				/// <summary>
				/// Upper bound color to match the close button which is positioned at the top right of inspection windows
				/// </summary>
				public static Color CloseButtonColorUpperBound = Color.FromArgb(80, 15, 15);

				/// <summary>
				/// Generate a marker bitmap based on the inspection type
				/// </summary>
				/// <param name="inspectionType">The inspection type, determining the final marker scale</param>
				/// <remarks><see cref="Config.Processing.Scale"/> is already accounted for.</remarks>
				/// <returns>A rescaled and alpha blended version of <see cref="Marker"/></returns>
				internal static Bitmap GetScaledMarker(InspectionProcessing.InspectionType inspectionType)
				{
					var markerScale = inspectionType switch
					{
						InspectionProcessing.InspectionType.Item => MarkerItemScale,
						InspectionProcessing.InspectionType.Container => MarkerContainerScale,
						InspectionProcessing.InspectionType.Unknown => throw new InvalidOperationException(),
					};

					var output = Marker.Rescale(markerScale * Scale);
					return output.Transparent2Color(MarkerBackgroundColor);
				}

				/// <summary>
				/// Scaled horizontal offset of the inspection window title search box
				/// <para>See <see cref="HorizontalTitleSearchOffsetFactor"/>.</para>
				/// </summary>
				/// <param name="inspectionType"></param>
				/// <returns></returns>
				internal static int GetHorizontalTitleSearchOffset(InspectionProcessing.InspectionType inspectionType)
				{
					var width = GetScaledMarker(inspectionType).Width;
					return (int)(width * HorizontalTitleSearchOffsetFactor);
				}
			}
		}
	}
}
