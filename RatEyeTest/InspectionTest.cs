using System.Drawing;
using RatEye;
using RatEye.Processing;
using Xunit;

namespace RatEyeTest
{
	[Collection("SerialTest")]
	public class InspectionTest : TestEnvironment
	{
		[Fact]
		public void ItemFHD()
		{
			var image = new Bitmap("TestData/FHD_Item.png");
			var inspType = Inspection.InspectionType.Item;
			var title = "GSSh-01 active headset";
			ConductTest(1f, image, inspType, 17, 13, title, "5b432b965acfc47a8774094e");
		}

		[Fact]
		public void ItemUHD()
		{
			var image = new Bitmap("TestData/UHD_Item.png");
			var inspType = Inspection.InspectionType.Item;
			var title = "TerraGroup Labs access keycard";
			ConductTest(2f, image, inspType, 79, 50, title, "5c94bbff86f7747ee735c08f");
		}

		[Fact]
		public void ContainerFHD()
		{
			var image = new Bitmap("TestData/FHD_Container.png");
			var inspType = Inspection.InspectionType.Container;
			var title = "Tri-Zip";
			ConductTest(1f, image, inspType, 19, 15, title, "545cdae64bdc2d39198b4568");
		}

		[Fact]
		public void ContainerUHD()
		{
			var image = new Bitmap("TestData/UHD_Container.png");
			var inspType = Inspection.InspectionType.Container;
			var title = "Holodilnick";
			ConductTest(2f, image, inspType, 51, 38, title, "5c093db286f7740a1b2617e3");
		}

		private static void ConductTest(
			float scale,
			Bitmap image,
			Inspection.InspectionType inspectionType,
			int posX,
			int posY,
			string title,
			string id)
		{
			var overrideConfig = new Config()
			{
				ProcessingConfig = new Config.Processing()
				{
					Scale = scale,
					InspectionConfig = new Config.Processing.Inspection()
					{
						MarkerThreshold = 0.99f,
						EnableContainers = true,
					}
				}
			}.Apply();

			var inspection = new Inspection(image, overrideConfig);
			Assert.True(inspection.ContainsMarker);
			Assert.InRange(inspection.MarkerConfidence, 0.99f, 1.0f);
			Assert.Equal(inspectionType, inspection.DetectedInspectionType);
			Assert.Equal(posX, inspection.MarkerPosition.X);
			Assert.Equal(posY, inspection.MarkerPosition.Y);
			Assert.InRange(inspection.Title.NormedLevenshteinDistance(title), 0.5f, 1f);
			Assert.Equal(id, inspection.Item.Id);
		}
	}
}
