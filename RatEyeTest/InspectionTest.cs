using System;
using System.Drawing;
using RatEye;
using RatEye.Processing;
using Xunit;

namespace RatEyeTest
{
	public class InspectionTest : TestEnvironment
	{
		[Fact]
		public void ItemFHD()
		{
			var image = new Bitmap("TestData/FHD_Item.png");
			var inspType = Inspection.InspectionType.Item;
			var title = "GSSh-01 active headset";
			ConductTest(1f, image, inspType, 17, 13, title);
		}

		[Fact]
		public void ItemUHD()
		{
			var image = new Bitmap("TestData/UHD_Item.png");
			var inspType = Inspection.InspectionType.Item;
			var title = "TerraGroup Labs access keycard";
			ConductTest(2f, image, inspType, 79, 50, title);
		}

		[Fact]
		public void ContainerFHD()
		{
			var image = new Bitmap("TestData/FHD_Container.png");
			var inspType = Inspection.InspectionType.Container;
			var title = "Tri-Zip";
			ConductTest(1f, image, inspType, 19, 15, title);
		}

		[Fact]
		public void ContainerUHD()
		{
			var image = new Bitmap("TestData/UHD_Container.png");
			var inspType = Inspection.InspectionType.Container;
			var title = "Holodilnick";
			ConductTest(2f, image, inspType, 51, 38, title);
		}

		private static void ConductTest(float scale, Bitmap image, Inspection.InspectionType inspectionType, Int32 posX, int posY, string title)
		{
			Config.Processing.Scale = scale;

			var inspection = new Inspection(image);
			Assert.True(inspection.ContainsMarker);
			Assert.True(inspection.MarkerConfidence > Config.Processing.Inspection.MarkerThreshold);
			Assert.Equal(inspectionType, inspection.DetectedInspectionType);
			Assert.Equal(posX, inspection.MarkerPosition.X);
			Assert.Equal(posY, inspection.MarkerPosition.Y);
			Assert.InRange(inspection.Title.NormedLevenshteinDistance(title), 0.5f, 1f);
		}
	}
}
