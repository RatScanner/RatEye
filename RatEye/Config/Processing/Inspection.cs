using System.Drawing;
using System.IO;
using RatEye.Properties;

namespace RatEye
{
	public partial class Config
	{
		public partial class Processing
		{
			/// <summary>
			/// The Inspection class contains parameters, used by the inspection processing module
			/// </summary>
			public class Inspection
			{
				/// <summary>
				/// Marker bitmap to identify regions of interest. This should be a cropped image of the magnifier icon
				/// </summary>
				public Bitmap Marker;

				/// <summary>
				/// Detection threshold of the marker bitmap
				/// </summary>
				public float MarkerThreshold;

				/// <summary>
				/// If true, allows to process the inspection window of containers
				/// </summary>
				public bool EnableContainers;

				/// <summary>
				/// The scale of the marker used by item inspection windows
				/// </summary>
				public float MarkerItemScale;

				/// <summary>
				/// The scale of the marker used by container inspection windows
				/// </summary>
				public float MarkerContainerScale;

				/// <summary>
				/// The background color used for the marker if it uses a alpha channel
				/// </summary>
				public Color MarkerBackgroundColor;

				/// <summary>
				/// Unscaled width of the box which will be searched for the title
				/// </summary>
				public int BaseTitleSearchWidth;

				/// <summary>
				/// Unscaled height of the box which will be searched for the title
				/// </summary>
				public int BaseTitleSearchHeight;

				/// <summary>
				/// Right padding of the title search box, used when the close button got detected
				/// <para>
				/// This helps to ignore extra buttons like the sort buttons in some container inspection windows.
				/// </para>
				/// See <see cref="CloseButtonColorLowerBound"/> and <see cref="CloseButtonColorUpperBound"/>.
				/// </summary>
				public int BaseTitleSearchRightPadding;

				/// <summary>
				/// The horizontal offset factor of the box which will be searched for the title
				/// <para>
				/// The factor is applied to the scaled width of the <see cref="Marker"/>.
				/// The horizontal offset is originating from the left most edge of the detected marker.
				/// <code>searchBox.left = detectedMarker.left + (Marker.width * Scale)</code>
				/// </para>
				/// </summary>
				public float HorizontalTitleSearchOffsetFactor;

				/// <summary>
				/// Lower bound color to match the close button which is positioned at the top right of inspection windows
				/// </summary>
				public Color CloseButtonColorLowerBound;

				/// <summary>
				/// Upper bound color to match the close button which is positioned at the top right of inspection windows
				/// </summary>
				public Color CloseButtonColorUpperBound;

				/// <summary>
				/// Create a new inspection config instance based on the state of <see cref="Config.GlobalConfig"/>
				/// </summary>
				/// <param name="basedOnDefault">
				/// Base the state on the default config rather then <see cref="Config.GlobalConfig"/>
				/// </param>
				public Inspection(bool basedOnDefault = false)
				{
					EnsureStaticInit();

					if (basedOnDefault)
					{
						SetDefaults();
						return;
					}

					var globalConfig = GlobalConfig.ProcessingConfig.InspectionConfig;

					Marker = globalConfig.Marker;
					MarkerThreshold = globalConfig.MarkerThreshold;
					EnableContainers = globalConfig.EnableContainers;
					MarkerItemScale = globalConfig.MarkerItemScale;
					MarkerContainerScale = globalConfig.MarkerContainerScale;
					MarkerBackgroundColor = globalConfig.MarkerBackgroundColor;
					BaseTitleSearchWidth = globalConfig.BaseTitleSearchWidth;
					BaseTitleSearchHeight = globalConfig.BaseTitleSearchHeight;
					BaseTitleSearchRightPadding = globalConfig.BaseTitleSearchRightPadding;
					HorizontalTitleSearchOffsetFactor = globalConfig.HorizontalTitleSearchOffsetFactor;
					CloseButtonColorLowerBound = globalConfig.CloseButtonColorLowerBound;
					CloseButtonColorUpperBound = globalConfig.CloseButtonColorUpperBound;
				}

				private void SetDefaults()
				{
					Marker = new Bitmap(new MemoryStream(Resources.icon_search));
					MarkerThreshold = 0.95f;
					EnableContainers = true;
					MarkerItemScale = 17f / 19f;
					MarkerContainerScale = 1f;
					MarkerBackgroundColor = Color.FromArgb(25, 27, 27);
					BaseTitleSearchWidth = 500;
					BaseTitleSearchHeight = 17;
					BaseTitleSearchRightPadding = 64;
					HorizontalTitleSearchOffsetFactor = 1.2f;
					CloseButtonColorLowerBound = Color.FromArgb(50, 10, 10);
					CloseButtonColorUpperBound = Color.FromArgb(80, 15, 15);
				}

				internal static void SetStaticDefaults() { }

				internal void Apply() { }
			}
		}
	}
}
